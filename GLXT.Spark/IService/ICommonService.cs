using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GLXT.Spark.Entity;
using GLXT.Spark.Entity.RSGL;
using GLXT.Spark.Entity.XTGL;
using GLXT.Spark.Model.Flow;
using GLXT.Spark.ViewModel.RSGL.Person;

namespace GLXT.Spark.IService
{
    public interface ICommonService
    {

        #region rsgl 人事管理

        #region Person 人员信息
        public List<Person> GetPersonList();
        /// <summary>
        /// 查询对应组织机构下的人员组成
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="postId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<PersonPost> GetPersonListByOrgId(int orgId, int postId = 0, string name = "");
        /// <summary>
        /// 根据人员信息查询对应的组织机构
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<int> GetPersonOrgIds(int postId = 0, string name = "");
        /// <summary>
        /// 查询对应组织机构下的岗位
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<PostPoolDetail> GetPostListByOrgId(int id);
        #endregion

        #region Post 岗位信息
        public List<Post> GetPostList(string name = "");
        public string GetPostNameById(int id);
        #endregion

        #region PostPool 岗位池信息
        public PostPool GetPostPoolById(int id);

        public List<PostPool> GetPostPoolList(string name = "");

        public List<PostPoolDetail> GetPostPoolDetailByOrg(int orgId);

        #endregion

        #region 用户岗位
        public List<PersonPost> GetOtherPersonPostById(int id);
        #endregion
        #endregion

        #region 泛型 缓存方法
        /// <summary>
        /// 查询并缓存人员基本信息
        /// </summary>
        /// <param name="companyId">公司Id</param>
        /// <returns>List</returns>
        public List<Person> GetCachePersonBasicInfoList(int companyId);
        /// <summary>
        /// 泛型 自动根据类型 获取缓存列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetCacheList<T>() where T : class;
        /// <summary>
        /// 根据key设置List
        /// </summary>
        /// <typeparam name="T">class</typeparam>
        /// <param name="keyName"></param>
        /// <param name="list">数据</param>
        public void SetCache<T>(string keyName, List<T> list);
        /// <summary>
        /// 根据key获取cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public List<T> GetCache<T>(string keyName);
        /// <summary>
        /// 根据key移除缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyName"></param>
        public void RemoveCache<T>(string keyName);

        /// <summary>
        /// 泛型 自动根据类型 删除缓存
        /// </summary>
        /// <typeparam name="T">类名</typeparam>
        public void RemoveCache<T>() where T : class;
        #endregion


        
    }
}