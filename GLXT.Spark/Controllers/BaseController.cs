using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GLXT.Spark.Model;
using GLXT.Spark.Model.Person;

namespace GLXT.Spark.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class BaseController : ControllerBase
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public string GetUserName()
        {
            return User.FindFirst(ClaimTypes.Name).Value;
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        public int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        public ClaimsModel GetUser()
        {
            return new ClaimsModel
            {
                Id = int.Parse(User.FindFirst(ClaimTypes.Sid).Value),
                Name = User.FindFirst(ClaimTypes.Name).Value,
                Number = User.FindFirst("Number").Value,
                Role = User.FindFirst(ClaimTypes.Role).Value,
                LogId = int.Parse(User.FindFirst("LogId").Value)
            };
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        // list 转 树形 
        public static List<TreeModel> GetTree(int printId, List<TreeModel> node)
        {
            List<TreeModel> mainNodes = node.Where(x => x.Pid == printId).ToList();
            foreach (var dpt in mainNodes)
            {
                dpt.Children = GetTree(dpt.Id, node);
            }
            return mainNodes;
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        // 递归 在指定节点中 是否包含指定节点
        public static List<TreeModel> GetNode(int id, List<TreeModel> node, int NodeId, List<TreeModel> nodetv)
        {
            List<TreeModel> mainNodes = node.Where(x => x.Pid == id).ToList();
            foreach (var dpt in mainNodes)
            {
                if (dpt.Id == NodeId)
                {
                    nodetv.Add(dpt);
                }
                else
                    GetNode(dpt.Id, node, NodeId, nodetv);
            }
            return nodetv;
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        // 递归获取所有指定节点下面的 叶子节点
        public static List<TreeModel> GetChildrenNodes(int id, List<TreeModel> list, List<TreeModel> ChildrenNodes)
        {
            List<TreeModel> mainNodes = list.Where(x => x.Pid == id).ToList();
            foreach (var dpt in mainNodes)
            {
                var childNodes = list.Where(w => w.Pid.Equals(dpt.Id));
                if (childNodes.Count() == 0)
                {
                    ChildrenNodes.Add(dpt);
                }
                dpt.Children = GetChildrenNodes(dpt.Id, list, ChildrenNodes);
            }
            return ChildrenNodes;
        }
    }
}
