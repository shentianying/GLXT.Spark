using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GLXT.Spark.Entity.RSGL;
using GLXT.Spark.Entity.XTGL;
using GLXT.Spark.Model.Person;
using GLXT.Spark.ViewModel.RSGL;
using GLXT.Spark.ViewModel.RSGL.Person;

namespace GLXT.Spark.IService
{
    /// <summary>
    /// 系统管理
    /// </summary>
    public interface ISystemService
    {


        #region Users 用户信息
        public string GetToken(Person person, int logId);
        public PersonViewModel GetPersonById(int id);
        #endregion


        #region Page 菜单页面
        public List<Page> GetPageList(string name = "");
        #endregion

        #region Role 角色
        public List<Role> GetRole(int? roleId, string name = "");
        #endregion

        #region UserRole 用户角色
        /// <summary>
        /// 获取用户角色 根据用户id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<UserRole> GetUserRole(int userId);
        /// <summary>
        /// 获取我和我的授权用户
        /// </summary>
        /// <returns></returns>
        // public List<int> GetMeAndMyAuthPerson();
        #endregion

        #region Permit 权限
        public List<Permit> GetPermitList(string name = "");
        #endregion


        #region RolePerimt 角色权限
        public List<RolePermit> GetRolePermitByRoleId(int[] roleIds);
        #endregion

        #region UserOrganization 用户查看范围权限
        public int GetCurrentSelectedCompanyId();
        public List<UserOrganization> GetUserOrganizationList(List<int> userIds);
        /// <summary>
        /// 获取用户，数据范围下面，勾选节点下面的所有子节点
        /// </summary>
        /// <returns></returns>
        public List<Organization> GetUserOrgChildList(int uid);
        #endregion

        #region UserCheckupOrganization 用户审核范围权限
        public List<UserCheckupOrganization> GetUserCheckupOrganization(UserCheckupOrganization userCheckupOrganization);
        #endregion

        #region Organization 组织机构
        public List<Organization> GetOrganizationList(Organization organization);

        /// <summary>
        /// 根据名称类别等搜索组织机构
        /// </summary>
        /// <param name="osvm"></param>
        /// <returns></returns>
        public List<Organization> GetOrganizationList(OrganizationSearchViewModel osvm);

        /// <summary>
        /// 根据Id查询组织机构
        /// </summary>
        /// <param name="ids">组织机构Id集合</param>
        /// <returns></returns>
        public List<Organization> GetOrgWithChildren(params int[] ids);

        /// <summary>
        /// 根据Id查询指定公司下的组织机构
        /// </summary>
        /// <param name="companyId">公司Id</param>
        /// <param name="ids">组织机构Id集合</param>
        /// <returns></returns>
        public List<Organization> GetComOrgWithChildren(int companyId, params int[] ids);

        /// <summary>
        /// 根据子节点Ids找到所有根节点
        /// </summary>
        /// <param name="ids">组织机构Id集合</param>
        /// <returns></returns>
        public List<int> GetOrgWithChildrenIds(params int[] ids);
        /// <summary>
        /// 给定组织机构中根据Id查询（包含所有父节点）
        /// </summary>
        /// <param name="orgLists">给定组织机构列表</param>
        /// <param name="ids">组织机构Id集合</param>
        /// <returns></returns>
        public List<Organization> GetOrgWithParents(List<Organization> orgLists, params int[] ids);
        /// <summary>
        /// 根据子节点Ids找到所有根节点对应的组织机构
        /// </summary>
        /// <param name="ids">组织机构Id集合</param>
        /// <returns></returns>
        public List<Organization> GetOrgRootToParent(params int[] ids);
        /// <summary>
        /// 根据子节点Id找到所有父节点字符串 例：公司/部门/人事部/员工
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public string GetOrgStringByLeafNode(int orgId);

        /// <summary>
        /// 对应公司下项目所对应的组织机构（不包括项目）
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<Organization> GetOrgWithProject(int companyId);

        /// <summary>
        /// 根据当前组织机构父节点ID获取统计余额的父节点组织机构
        /// </summary>
        /// <param name="orgPId"></param>
        /// <returns></returns>
        public Organization GetOrgIsSumByPId(int orgPId);

        /// <summary>
        /// 获取统计余额的组织机构子集
        /// </summary>
        /// <param name="orgList">组织机构</param>
        /// <param name="orgId">父ID</param>
        /// <returns></returns>
        public List<Organization> GetSumOrgList(List<Organization> orgList, int orgId);
        #endregion

        #region 树形查询

        /// <summary>
        /// 查询Id数组中所有节点及子节点
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="list">树形全集，必须包含（Id，PId属性）</param>
        /// <param name="Ids">节点Id集合</param>
        /// <returns></returns>
        public List<T> GetWithChildren<T>(List<T> list, params int[] Ids);
        public List<Organization> GetOrgWithParents(params int[] ids);

        #endregion

        #region 字典数据
        /// <summary>
        /// 根据类型获取字典数据
        /// </summary>
        /// <param name="type">字典数据类型</param>
        /// <returns></returns>
        public List<Dictionary> GetDictionary(string type = "");

        /// <summary>
        /// 返回字典中指定值下的所有子项（用户当前公司）
        /// </summary>
        /// <param name="type">字典类型</param>
        /// <param name="values">值的集合</param>
        /// <returns></returns>
        public List<Dictionary> GetDictionaryWithChildren(string type, params int[] values);

        /// <summary>
        /// 返回字典数据中指定类型下所对应值的名称
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string GetDictionaryWithName(string type, int value);

        /// <summary>
        /// 根据类型名称获取类型数据
        /// 含有筛选项的排除
        /// </summary>
        /// <param name="values">筛选value值</param>
        /// <param name="type">类型名称</param>
        /// <returns></returns>
        public List<Dictionary> GetDictionariesWithoutValues(int[] values, string type = "");
        #endregion

        #region City 地区
        public List<City> GetCity();
        #endregion

        #region UpFile 上传文件
        public bool UpdateFile<T>(List<ViewModel.XTGL.UpFile.FileList> fileLists, int tableId = 0, string columnName = "") where T : class;
        public bool AddFiles<T>(List<ViewModel.XTGL.UpFile.FileList> fileLists, int tableId = 0, string columnName = "") where T : class;
        #endregion

        /// <summary>
        /// 生成单据编号
        /// </summary>
        /// <typeparam name="T">单据类型</typeparam>
        /// <param name="str">编号前缀</param>
        /// <param name="lenght">流水号长度</param>
        /// <returns>单据编号</returns>
        public string GetNewBillNumber<T>(string str, int lenght) where T : class;


        #region 异常信息
        public bool AddExceptions(SystemExceptions se);
        #endregion

        #region 消息Message
        //public bool SendMessage(List<Message> messages);
        //public bool SendMessage(Message message);
        #endregion
    }
}
