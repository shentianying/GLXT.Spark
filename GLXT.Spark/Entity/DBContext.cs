using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLXT.Spark.Entity.RSGL;
using GLXT.Spark.Entity.XTGL;
using GLXT.Spark.IService;

namespace GLXT.Spark.Entity
{
    public class DBContext : DbContext
    {
        private readonly IPrincipalAccessor _principalAccessor;
        public DBContext(DbContextOptions<DBContext> options, IPrincipalAccessor principalAccessor)
            : base(options)
        {
            _principalAccessor = principalAccessor;
        }
        //public DbSet<Users> Users { get; set; } 待删除

        #region rsgl人事管理
        public DbSet<Post> Post { get; set; }
        public DbSet<PostPool> PostPool { get; set; }
        public DbSet<PostPoolDetail> PostPoolDetail { get; set; }
        public DbSet<PersonPost> PersonPost { get; set; }
        #endregion

        #region xtgl系统管理

        #region xtgl 权限
        public DbSet<Person> Person { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<Permit> Permit { get; set; }
        public DbSet<UserRole> UserRole { get; set; }
        public DbSet<RolePermit> RolePermit { get; set; }
        public DbSet<UserOrganization> UserOrganization { get; set; }
        public DbSet<UserCheckupOrganization> UserCheckupOrganization { get; set; }
        public DbSet<Log> Log { get; set; }

        #endregion

        #region xtgl 基本信息
        public DbSet<Page> Page { get; set; }
        public DbSet<Dictionary> Dictionary { get; set; }
        public DbSet<Organization> Organization { get; set; }
        public DbSet<AccountSet> AccountSet { get; set; }
        public DbSet<UpFile> UpFile { get; set; }
        public DbSet<UpFileTemp> UpFileTemp { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<SystemExceptions> SystemExceptions { get; set; }
        public DbSet<Remind> Remind { get; set; }
        public DbSet<Message> Message { get; set; }
        public DbSet<MessageUser> MessageUser { get; set; }

        #endregion

        #region xtgl 流程管理
        public DbSet<Attitude> Attitude { get; set; }
        public DbSet<Flow> Flow { get; set; }
        public DbSet<FlowCondition> FlowCondition { get; set; }
        public DbSet<FlowNode> FlowNode { get; set; }
        public DbSet<FormStateOption> FormStateOption { get; set; }
        public DbSet<FormState> FormState { get; set; }
        public DbSet<Form> Form { get; set; }
        public DbSet<FormFlowField> FormFlowField { get; set; }
        public DbSet<BillFlow> BillFlow { get; set; }
        public DbSet<BillFlowNode> BillFlowNode { get; set; }
        #endregion

        #region jcsj
        public DbSet<OtherMaterial> OtherMaterial { get; set; }
        #endregion

        #endregion


        public static readonly LoggerFactory LoggerFactory =
               new LoggerFactory(new[] { new DebugLoggerProvider() });
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //if (!optionsBuilder.IsConfigured)
            //{
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseLoggerFactory(LoggerFactory);
            //optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=SGL;User ID=sa;Password=1");
            //optionsBuilder.UseSqlServer("Data Source=172.10.11.33;Initial Catalog=Spark;User ID=xuefei;Password=b4ktfQszOnrI9eON");
            //}
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }

        public new int SaveChanges()
        {
            //WriteEFDataLog();
            return base.SaveChanges();
        }

        /// <summary>
        /// EF变更日志
        /// </summary>
        private void WriteEFDataLog()
        {
            //定义不跟踪的表
            string[] noTrackTable = { "xtglLog", "xtglLogDetail" };
            //获取到EF变更条目
            var list = this.ChangeTracker.Entries();
            foreach (var item in list)
            {
                //对应的表名
                string tableName = "";

                Type type = item.Entity.GetType();
                Type patientMngAttrType = typeof(TableAttribute);
                TableAttribute attribute = null;
                if (type.IsDefined(patientMngAttrType, true))
                {
                    attribute = type.GetCustomAttributes(patientMngAttrType, true).FirstOrDefault() as TableAttribute;
                    if (attribute != null)
                    {
                        tableName = attribute.Name;
                    }
                }

                if (string.IsNullOrEmpty(tableName))
                {
                    tableName = type.Name;
                }

                if (noTrackTable.Contains(tableName))
                {
                    return;
                }
                switch (item.State)
                {
                    case EntityState.Detached:

                        break;
                    case EntityState.Unchanged:

                        break;
                    case EntityState.Deleted:

                        break;
                    case EntityState.Modified:
                        WriteEFUpdateLog(item, tableName);
                        break;
                    case EntityState.Added:

                        break;
                }
            }
        }

        /// <summary>
        /// 记录EF修改操作日志
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="tableName"></param>
        private void WriteEFUpdateLog(EntityEntry entry, string tableName)
        {
            var propertyList = entry.CurrentValues.Properties.Where(i => entry.Property(i.Name).IsModified);

            PropertyEntry keyEntity = entry.Property("Id");

            StringBuilder sb = new StringBuilder();
            foreach (var prop in propertyList)
            {
                if (prop != null)
                {
                    PropertyEntry entity = entry.Property(prop.Name);
                    // 这里写保存方法
                    string userName = _principalAccessor.Claim().Name;
                    if (entity.OriginalValue == null || entity.CurrentValue == null)
                    {
                        continue;
                    }
                    else
                    {
                        if (!entity.OriginalValue.Equals(entity.CurrentValue))
                        {
                            string log = $"用户：{ userName }，对表：{ tableName }中的字段：{prop.Name} 进行了修改，原始值：{ entity.OriginalValue }，当前值：{  entity.CurrentValue }, 唯一标识：{ keyEntity.CurrentValue }";
                            sb.Append($" \t\n{ EntityState.Modified }: 创建时间：{ DateTime.Now } 日志内容：{ log }");
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(sb.ToString()))
            {
                string path = AppContext.BaseDirectory + "EFCoreUpdateLog.txt";
                File.AppendAllText(path, sb.ToString());
            }
        }

    }
}
