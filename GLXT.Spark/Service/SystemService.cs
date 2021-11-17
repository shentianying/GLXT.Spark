using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using GLXT.Spark.Entity;
using GLXT.Spark.Entity.RSGL;
using GLXT.Spark.Entity.XTGL;
using GLXT.Spark.IService;
using GLXT.Spark.Model;
using GLXT.Spark.Utils;
using GLXT.Spark.ViewModel.RSGL;
using GLXT.Spark.ViewModel.RSGL.Person;

namespace GLXT.Spark.Service
{
    /// <summary>
    /// 系统管理
    /// </summary>
    public class SystemService : ISystemService
    {
        private readonly DBContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _memoryCache;
        private readonly TokenManagement _tokenManagement;
        private readonly AppSettingModel _appSettingModel;
        private readonly IPrincipalAccessor _principalAccessor;
        private readonly ICommonService _commonService;
        public SystemService(
            DBContext dBContext,
            IMapper mapper,
            IMemoryCache memoryCache,
            IOptions<TokenManagement> tokenManagement,
            IOptions<AppSettingModel> appSettingModel,
            ICommonService commonService,
            IPrincipalAccessor principalAccessor)
        {
            _dbContext = dBContext;
            _mapper = mapper;
            _memoryCache = memoryCache;
            _principalAccessor = principalAccessor;
            _tokenManagement = tokenManagement.Value;
            _appSettingModel = appSettingModel.Value;
            _commonService = commonService;
        }

        #region 用户信息

        #region token
        public enum TokenType
        {
            AccessToken = 1,
            RefreshToken = 2
        }
        public string GetToken(Person person, int logId)
        {
            return CreateToken(person, logId, TokenType.AccessToken).Token;
        }
        public TokenViewModel RefreshToken(Person person, int logId)
        {
            return CreateToken(person, logId, TokenType.RefreshToken);
        }
        private TokenViewModel CreateToken(Person person, int logId, TokenType tokenType)
        {
            string token = string.Empty;
            var claims = new[]
            {
                new Claim(ClaimTypes.Name,person.Name),
                new Claim("Number",person.Number),
                new Claim(ClaimTypes.Sid,person.Id.ToString()),
                new Claim(ClaimTypes.Role,string.Join(",", person.UserRoles.Select(s=>s.RoleId))),
                new Claim("LogId",logId.ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenManagement.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = tokenType.Equals(TokenType.AccessToken) ? DateTime.Now.AddMinutes(_tokenManagement.AccessExpiration) : DateTime.Now.AddMinutes(_tokenManagement.RefreshExpiration);
            var jwtToken = new JwtSecurityToken(
                _tokenManagement.Issuer,
                tokenType.Equals(TokenType.AccessToken) ? _tokenManagement.Audience : _tokenManagement.RefreshAudience,
                claims,
                expires: expires,
                signingCredentials: credentials
                );
            token = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            return new TokenViewModel
            {
                Token = token,
                Expires = expires,
                UserName = person.Number
            };
        }
        public PersonViewModel GetPersonById(int id)
        {
            var query = _dbContext.Person.Find(id);
            var result = _mapper.Map<PersonViewModel>(query);
            if (result.PostId != null)
            {
                result.Post = _dbContext.Post.Find(result.PostId);
            }
            result.Organization = _dbContext.Organization.Find(result.OrganizationId);
            return result;
        }
        #endregion

        #endregion

        #region Page 菜单页面
        public List<Page> GetPageList(string name = "")
        {
            List<Page> query = _commonService.GetCacheList<Page>().OrderBy(o => o.Sort).ToList();
            if (name != "") query = query.Where(w => w.RouterTitle.Equals(name)).ToList();
            return query;
        }
        #endregion

        #region Role 角色
        /// <summary>
        /// 获取 所有角色
        /// </summary>
        /// <param name="roleId">角色id</param>
        /// <param name="name">角色名称</param>
        /// <returns></returns>
        public List<Role> GetRole(int? roleId, string name = "")
        {
            var cId = GetCurrentSelectedCompanyId();
            var query = _dbContext.Role.Where(i => i.Status.Equals(1) && i.CompanyId.Equals(cId));
            if (roleId.HasValue)
            {
                query = query.Where(w => w.Id.Equals(roleId.Value));
            }
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(w => w.Name.Contains(name));
            }
            return query.AsNoTracking().ToList();
        }
        #endregion

        #region UserRole 用户角色
        public List<UserRole> GetUserRole(int id)
        {
            return _dbContext.UserRole
                .Include(i => i.Role)
                .Where(w => w.UserId.Equals(id))
                .ToList();
        }
        public bool AddUserRole(UserRole userRole)
        {
            _dbContext.UserRole.Add(userRole);
            return _dbContext.SaveChanges() > 0;
        }


        #endregion

        #region UserPerimt  角色权限
        public List<RolePermit> GetRolePermitByRoleId(int[] roleIds)
        {
            var list = _commonService.GetCache<RolePermit>("GetRolePermit");
            if (list == null)
            {
                var query = _dbContext
                    .RolePermit
                    .OrderBy(o => o.Id)
                    .Include(i => i.Permit)
                    .ToList();
                _commonService.SetCache<RolePermit>("GetRolePermit", query);
                list = query;
            }
            var result = list.Where(w => roleIds.Contains(w.RoleId));
            return result.ToList();
        }
        #endregion

        #region Permit 权限
        public List<Permit> GetPermitList(string name = "")
        {
            return _dbContext.Permit.OrderBy(o => o.Sort).ToList();
        }

        #region 权限验证
        /// <summary>
        /// 判断当前用户是否有某项权限
        /// </summary>
        /// <param name="permitCode"></param>
        /// <returns></returns>
        public bool IsAuth(string permitCode)
        {
            //获取用户的角色
            var roleIds = _principalAccessor.Claim().Role;
            // string 数组 转化 int 数组
            var convertIntArr = Array.ConvertAll<string, int>(roleIds.Split(',', StringSplitOptions.None), s => int.Parse(s));
            // 根据角色获取 角色权限List
            var rolePermitList = GetRolePermitByRoleId(convertIntArr);

            return rolePermitList.Any(a => a.Permit.Code == permitCode);
        }
        #endregion
        #endregion

        #region UserOrganization 用户查看范围权限

        public int GetCurrentSelectedCompanyId()
        {
            var result = GetUserOrganizationList(new List<int> { _principalAccessor.Claim().Id });
            return result.FirstOrDefault(s => s.Selected).CompanyId;
        }

        public List<UserOrganization> GetUserOrganizationList(List<int> userIds)
        {
            var query = _dbContext.UserOrganization
                .Where(w => userIds.Contains(w.UserId))
                .AsNoTracking();
            return query.ToList();
        }

        /// <summary>
        /// 获取用户，数据范围下面，勾选节点下面的所有子节点
        /// </summary>
        /// <returns></returns>
        public List<Organization> GetUserOrgChildList(int uid)
        {
            int companyId = GetCurrentSelectedCompanyId();
            var userOrgList = GetUserOrganizationList(new List<int> { uid });

            if (userOrgList == null)
                return null;

            // 获取用户数据范围orgIds下的所有子节点
            var orgIds = userOrgList.Select(s => s.OrganizationId).ToArray();
            return GetComOrgWithChildren(companyId, orgIds);
        }
        #endregion

        #region UserCheckupOrganization 用户审核范围权限
        public List<UserCheckupOrganization> GetUserCheckupOrganization(UserCheckupOrganization userCheckupOrganization)
        {
            var query = _dbContext.UserCheckupOrganization
                .Where(w => w.PersonId.Equals(userCheckupOrganization.PersonId));
            return query.ToList();
        }
        #endregion

        #region Organization 组织机构
        public List<Organization> GetOrganizationList(Organization organization)
        {
            IQueryable<Organization> org = _dbContext.Organization.Where(w => w.InUse);
            if (!string.IsNullOrWhiteSpace(organization.Name))
                org = org.Where(n => n.Name.Contains(organization.Name));
            if (organization.PId >= 0)
                org = org.Where(n => n.PId.Equals(organization.PId));
            return org.OrderBy(o => o.Sort).ToList();
        }

        public List<Organization> GetOrganizationList(OrganizationSearchViewModel osvm)
        {
            IQueryable<Organization> org = _dbContext.Organization.Where(w => w.InUse);
            if (!string.IsNullOrWhiteSpace(osvm.name))
                org = org.Where(n => n.Name.Contains(osvm.name));
            if (osvm.isProject.HasValue)
                org = org.Where(n => n.IsProject.Equals(osvm.isProject));
            if (osvm.categoryIds.Length > 0)
                org = org.Where(n => osvm.categoryIds.Contains(n.CategoryId));
            return org.OrderBy(o => o.Sort).ToList();
        }

        /// <summary>
        /// 根据Id查询组织机构（包含所有子节点）
        /// </summary>
        /// <param name="ids">组织机构Id集合</param>
        /// <returns></returns>
        public List<Organization> GetOrgWithChildren(params int[] ids)
        {
            List<Organization> list = _commonService.GetCacheList<Organization>().Where(w => w.InUse).ToList();
            return GetWithChildren(list, ids);
        }
        /// <summary>
        /// 根据Id查询指定公司下的组织机构
        /// </summary>
        /// <param name="companyId">公司Id</param>
        /// <param name="ids">组织机构Id集合</param>
        /// <returns></returns>
        public List<Organization> GetComOrgWithChildren(int companyId, params int[] ids)
        {
            List<Organization> list = GetOrgWithChildren(companyId);
            return GetWithChildren(list, ids);
        }


        /// <summary>
        /// 对应公司下项目所对应的组织机构（不包括项目）
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<Organization> GetOrgWithProject(int companyId)
        {
            //List<Organization> list = _commonService.GetCacheList<Organization>().Where(w => w.InUse && w.IsProject ).ToList();
            List<Organization> list = GetOrgWithChildren(companyId).Where(w => w.IsProject).ToList();
            List<int> obj = list.Select(o => o.Id).Distinct().ToList();
            list = GetOrgWithParents(obj.ToArray());
            List<Organization> orgList = new List<Organization>();
            foreach (Organization item in list)
            {
                if (!item.IsProject)
                    orgList.Add(item);
            }
            return orgList;
        }

        /// <summary>
        /// 根据Id查询组织机构（包含所有父节点）
        /// </summary>
        /// <param name="ids">组织机构Id集合</param>
        /// <returns></returns>
        public List<Organization> GetOrgWithParents(params int[] ids)
        {
            List<Organization> list = _commonService.GetCacheList<Organization>().Where(w => w.InUse).ToList();
            return GetWithParents(list, ids);
        }

        /// <summary>
        /// 给定组织机构中根据Id查询（包含所有父节点）
        /// </summary>
        /// <param name="orgLists">给定组织机构列表</param>
        /// <param name="ids">组织机构Id集合</param>
        /// <returns></returns>
        public List<Organization> GetOrgWithParents(List<Organization> orgLists, params int[] ids)
        {
            return GetWithParents(orgLists, ids);
        }

        /// <summary>
        /// 根据子节点Ids找到所有根节点
        /// </summary>
        /// <param name="ids">组织机构Id集合</param>
        /// <returns></returns>
        public List<int> GetOrgWithChildrenIds(params int[] ids)
        {
            List<int> rootNode = new List<int>();
            List<int> childNode = new List<int>();
            foreach (int id in ids)
            {
                List<Organization> orglists = _commonService.GetCacheList<Organization>().Where(w => w.InUse && w.PId.Equals(id)).ToList();
                if (orglists.Count == 0)
                    rootNode.Add(id);
                else
                {
                    foreach (var org in orglists)
                    {
                        childNode.Add(org.Id);
                    }
                    rootNode = rootNode.Concat(GetOrgWithChildrenIds(childNode.ToArray())).ToList();
                }
            }
            return rootNode;
        }

        /// <summary>
        /// 根据子节点Ids找到所有根节点对应的组织机构
        /// </summary>
        /// <param name="ids">组织机构Id集合</param>
        /// <returns></returns>
        public List<Organization> GetOrgRootToParent(params int[] ids)
        {
            List<Organization> list = _commonService.GetCacheList<Organization>().Where(w => w.InUse).ToList();
            return GetWithParents(list, GetOrgWithChildrenIds(ids).ToArray());
        }
        /// <summary>
        /// 根据子节点Id找到所有父节点字符串 例：公司/部门/人事部/员工
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public string GetOrgStringByLeafNode(int orgId)
        {
            var orgList = GetOrgWithParents(orgId);
            string orgStr = "";
            foreach (var org in orgList)
            {
                orgStr = org.Name + "/" + orgStr;
            }
            return orgStr.TrimEnd('/');
        }

        /// <summary>
        /// 根据当前组织机构父节点ID获取统计余额的父节点组织机构
        /// </summary>
        /// <param name="orgPId"></param>
        /// <returns></returns>
        public Organization GetOrgIsSumByPId(int orgPId)
        {
            Organization org = _commonService.GetCacheList<Organization>().FirstOrDefault(t => t.InUse && t.Id.Equals(orgPId));
            if (org != null && org.IsSum == true)
            {
                return org;
            }
            else if (org != null && org.IsSum == false)
            {
                return GetOrgIsSumByPId(org.PId);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取统计余额的组织机构子集
        /// </summary>
        /// <param name="orgList">组织机构</param>
        /// <param name="orgId">父ID</param>
        /// <returns></returns>
        public List<Organization> GetSumOrgList(List<Organization> orgList, int orgId)
        {
            var comOrglist = GetWithChildren(orgList, orgId);
            List<Organization> searchOrgList = new List<Organization>();
            foreach (var org in comOrglist)
            {
                var POrg = GetOrgIsSumByPId(org.Id);
                if (POrg!=null && orgId.Equals(POrg.Id))
                {
                    searchOrgList.Add(org);
                }
            }
            return searchOrgList;
        }
        #endregion

        #region 树形查询
        /// <summary>
        /// 查询Id数组中所有节点及子节点
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="list">树形全集，必须包含（Id，PId属性）</param>
        /// <param name="Ids">节点Id集合</param>
        /// <returns></returns>
        public List<T> GetWithChildren<T>(List<T> list, params int[] Ids)
        {
            return GetWithChildren(list.AsQueryable(), Ids).ToList<T>();
        }

        private IQueryable<T> GetWithChildren<T>(IQueryable<T> FullQuery, params int[] Ids)
        {
            if (Ids.Length == 0) return null;
            IQueryable<T> result = FullQuery.Where("@0.Contains(Id)", Ids);
            Ids = FullQuery.Where("@0.Contains(PId)", Ids).Select("Id").ToDynamicArray<int>();
            if (Ids.Length > 0)
                return result.Union(GetWithChildren(FullQuery, Ids));
            else
                return result;
        }

        /// <summary>
        /// 查询Id数组中所有节点及父节点
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="list">树形全集，必须包含（Id，PId属性）</param>
        /// <param name="Ids">节点Id集合</param>
        /// <returns></returns>
        public List<T> GetWithParents<T>(List<T> list, params int[] Ids)
        {
            return GetWithParents(list.AsQueryable(), Ids).ToList<T>();
        }

        private IQueryable<T> GetWithParents<T>(IQueryable<T> FullQuery, params int[] Ids)
        {
            if (Ids.Length == 0) return null;
            IQueryable<T> result = FullQuery.Where("@0.Contains(Id)", Ids);
            Ids = result.Where("PId != 0").Select("PId").ToDynamicArray<int>();
            if (Ids.Length > 0)
                return result.Union(GetWithParents(FullQuery, Ids));
            else
                return result;
        }
        #endregion

        #region dictionary 字典数据
        /// <summary>
        /// 更具类型获取字典数据
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public List<Dictionary> GetDictionary(string type = "")
        {
            var query = _commonService.GetCacheList<Dictionary>();
            var q1 = query.OrderBy(o => o.Type).ThenBy(tb => tb.Sort)
                .Where(w => w.InUse);
            if (!string.IsNullOrEmpty(type))
            {
                q1 = q1.Where(w => w.Type.Equals(type));
            }

            int[] arr = { 0, GetCurrentSelectedCompanyId() };// 0：公共的数据 + 各个公司特有的字段
            if (arr.Length > 0)
                q1 = q1.Where(w => arr.Contains(w.CompanyId));
            return q1.ToList();
        }

        /// <summary>
        /// 返回字典中指定值下的所有子项（用户当前公司）
        /// </summary>
        /// <param name="type">字典类型</param>
        /// <param name="values">值的集合</param>
        /// <returns></returns>
        public List<Dictionary> GetDictionaryWithChildren(string type, params int[] values)
        {
            List<Dictionary> list = GetDictionary(type);
            int[] Ids = list.Where(w => values.Contains(w.Value)).Select(s => s.Id).ToArray();

            return GetWithChildren(list, Ids);
        }

        /// <summary>
        /// 返回字典数据中指定类型下所对应值的名称
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string GetDictionaryWithName(string type, int value = 0)
        {
            string strName = "";
            var list = GetDictionary(type).Where(w => w.Value.Equals(value));
            if (list.Count() > 0)
                strName = list.First().Name;
            return strName;
        }

        /// <summary>
        /// 根据类型名称获取类型数据
        /// 含有筛选项的排除
        /// </summary>
        /// <param name="values">筛选value值</param>
        /// <param name="type">类型名称</param>
        /// <returns></returns>
        public List<Dictionary> GetDictionariesWithoutValues(int[] values, string type = "")
        {
            var query = _commonService.GetCacheList<Dictionary>();
            var q1 = query.OrderBy(o => o.Type).ThenBy(tb => tb.Sort)
                .Where(w => w.InUse);
            if (!string.IsNullOrEmpty(type))
            {
                q1 = q1.Where(w => w.Type.Equals(type));
            }
            if (values?.Length > 0)
            {
                foreach (var value in values)
                {
                    var q2 = q1.FirstOrDefault(w => w.Value.Equals(value));
                    if (q2 != null && q2.PId.Equals(0))
                    {
                        //父级排除所有子集
                        q1 = q1.Where(w => !w.Id.Equals(q2.Id) && !w.PId.Equals(q2.Id));
                    }
                    else if (q2 != null && !q2.PId.Equals(0))
                    {
                        //排除当前
                        q1 = q1.Where(w => !w.Id.Equals(q2.Id));
                    }
                }
            }

            int[] arr = { 0, GetCurrentSelectedCompanyId() };// 0：公共的数据 + 各个公司特有的字段
            if (arr.Length > 0)
                q1 = q1.Where(w => arr.Contains(w.CompanyId));
            return q1.ToList();
        }
        #endregion

        #region City 地区
        public List<City> GetCity()
        {
            var query = _commonService.GetCacheList<City>();
            var q1 = query.OrderBy(o => o.Sort)
                .Where(w => w.InUse);
            return q1.ToList();
        }
        #endregion

        #region UpFile 上传文件
        public bool UpdateFile<T>(List<ViewModel.XTGL.UpFile.FileList> fileLists, int tableId = 0, string columnName = "") where T : class
        {
            int[] upfileIds = new int[] { };
            if (fileLists?.Count > 0 && tableId != 0)
            {
                //编辑的文件列表中 有删除，新增，和不变 三种状态的文件
                upfileIds = fileLists.Where(w => !w.addFlag).Select(s => s.id).ToArray();
                var upfileTempIds = fileLists.Where(w => w.addFlag).Select(s => s.id).ToArray();
                // 上传新增的临时文件的处理 读取临时文件UpFileTemp数据，添加到正式文件表upfile中
                var q1 = _dbContext.UpFileTemp.Where(w => upfileTempIds.Contains(w.Id)).AsNoTracking().ToList();
                if (q1.Count > 0)
                {
                    q1.ForEach(e =>
                    {
                        _dbContext.Add(new UpFile
                        {
                            TableId = tableId,
                            //TableName = tableStartStr + typeof(T).Name,
                            TableName = Utils.Common.GetTableName<T>(),
                            FileName = e.FileName,
                            ColumnName = columnName,
                            FileValue = e.FileValue,
                            FilePath = e.FilePath,
                            FileType = e.FileType,
                            FileSize = e.FileSize,
                            CreateUserId = _principalAccessor.Claim().Id,
                            CreateUserName = _principalAccessor.Claim().Name,
                            LastEditUserId = _principalAccessor.Claim().Id,
                            LastEditUserName = _principalAccessor.Claim().Name
                        });
                    });
                    _dbContext.RemoveRange(q1);
                }
            }
            // 对比正式表upfile中的文件和提交过来修改的upfile 中的文件对比是否一致，没有就删除，有就不动(添加columnName判断，分类文件上传有bug——sty)
            var q2 = _dbContext.UpFile.Where(w => w.TableId.Equals(tableId) && w.ColumnName.Equals(columnName)).ToList();//
            if (q2.Any())
            {
                q2.ForEach(e =>
                {
                    if (!upfileIds.Contains(e.Id))
                    {
                        _dbContext.Remove(e);

                        // 同时删除文件（UpFileTemp表中的，上传未保存的临时文件要用 定时任务工具清除）
                        string path = System.IO.Path.Combine(e.FilePath, e.FileValue);
                        if (System.IO.File.Exists(path))
                            System.IO.File.Delete(path);

                    }
                });
            }
            return _dbContext.SaveChanges() > 0;
        }

        public bool AddFiles<T>(List<ViewModel.XTGL.UpFile.FileList> fileLists, int tableId = 0, string columnName = "") where T : class
        {
            if (fileLists == null)
                return false;
            if (fileLists.Count > 0)
            {
                var upfileTempIds = fileLists.Where(w => w.addFlag).Select(s => s.id).ToArray();
                // 上传文件中的临时文件的处理 读取临时文件UpFileTemp数据，添加到正式文件表upfile中
                var q1 = _dbContext.UpFileTemp.Where(w => upfileTempIds.Contains(w.Id)).AsNoTracking().ToList();
                foreach (var e in q1)
                {
                    var uf = new UpFile
                    {
                        TableId = tableId,
                        //TableName = tableStartStr + typeof(T).Name,
                        TableName = Utils.Common.GetTableName<T>(),
                        FileName = e.FileName,
                        ColumnName = columnName,
                        FileValue = e.FileValue,
                        FilePath = e.FilePath,
                        FileType = e.FileType,
                        FileSize = e.FileSize,
                        CreateUserId = _principalAccessor.Claim().Id,
                        CreateUserName = _principalAccessor.Claim().Name,
                        LastEditUserId = _principalAccessor.Claim().Id,
                        LastEditUserName = _principalAccessor.Claim().Name
                    };
                    _dbContext.Add(uf);
                };
                _dbContext.RemoveRange(q1);
                return _dbContext.SaveChanges() > 0;
            }
            return false;
        }
        #endregion

        #region 通用方法

        /// <summary>
        /// 生成单据编号
        /// </summary>
        /// <typeparam name="T">单据类型</typeparam>
        /// <param name="str">编号前缀</param>
        /// <param name="lenght">流水号长度</param>
        /// <returns>单据编号</returns>
        public string GetNewBillNumber<T>(string str, int lenght) where T : class
        {
            string comShort = _dbContext.Organization.Find(GetCurrentSelectedCompanyId()).ShortName;

            str = comShort + str;
            string strMax = (string)_dbContext.Set<T>().Where("Number.StartsWith(@0) && CompanyId.Equals(@1)", str, GetCurrentSelectedCompanyId()).Max("Number");
            int intMax;
            if (string.IsNullOrEmpty(strMax))
                intMax = 0;
            else
                intMax = Common.GetInt(strMax.Substring(strMax.Length - lenght));
            intMax++;

            strMax = intMax.ToString().PadLeft(lenght, '0');

            return str + strMax;
        }

        #endregion

        #region 异常
        public bool AddExceptions(SystemExceptions se)
        {
            _dbContext.Add(se);
            return _dbContext.SaveChanges() > 0;
        }
        #endregion

        #region 消息Message
        //public bool SendMessage(List<Message> messages)
        //{
        //    //int companyId = GetCurrentSelectedCompanyId();
        //    if (messages == null) return false;
        //    //foreach (var msg in messages)
        //    //{
        //    //    msg.CompanyId = companyId;
        //    //}
        //    _dbContext.AddRange(messages);
        //    var result = _dbContext.SaveChanges() > 0;
        //    return result;
        //}

        #endregion
    }
}
