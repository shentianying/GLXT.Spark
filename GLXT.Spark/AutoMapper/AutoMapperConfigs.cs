using AutoMapper;
using System.Linq;
using GLXT.Spark.Entity.RSGL;
using GLXT.Spark.Entity.XTGL;
using GLXT.Spark.Model;
using GLXT.Spark.ViewModel.RSGL.Person;
using GLXT.Spark.ViewModel.XTGL.Users;

namespace GLXT.Spark.AutoMapper
{
    public class AutoMapperConfigs:Profile
    {
        public AutoMapperConfigs()
        {
            CreateMap<Person, PersonViewModel>();
            //CreateMap<Users, GetUsersViewModel>()
            //    .ForMember(fm => fm.PostName, opt => opt.MapFrom(src => src.Post.Name))
            //    .ForMember(fm => fm.OrganizationName, opt => opt.MapFrom(src => src.Organization.Name))
            //    .ForMember(fm => fm.RoleNameList, opt => opt.MapFrom(src => src.UserRoles.Select(s=>s.Role.Name).ToArray()))
            //    ;
            //CreateMap<Users, UsersViewModel>()
            //    .ForMember(fm => fm.RoleNames, opt => opt.MapFrom(src => src.UserRoles.Select(s=>s.Role.Name).ToArray()));
        }
    }
}
