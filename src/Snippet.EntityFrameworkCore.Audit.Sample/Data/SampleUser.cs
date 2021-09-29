using Snippet.EntityFrameworkCore.Audit.Attribute;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snippet.EntityFrameworkCore.Audit.Sample.Data
{
    [TableAudit(true, "{User}添加了一个新用户：{Name}，{Account}，{Level}",
         "{User}更新了用户信息,主键为{Id},修改内容为{Account}")]
    [Table("t_testentity")]
    public class SampleUser
    {
        public int Id { get; set; }

        [ColumnAudit("用户名：{value}", "用户名:{original}->{current}")]
        [Column("c_testentity")]
        public string Name { get; set; }

        [ColumnAudit("账户金额：{value}", "账户金额:{original}->{current}")]
        [Column("c_account")]
        public string Account { get; set; }

        [ColumnAudit("会员等级：{value}", "会员等级:{original}->{current}")]
        [ColumnDic(1, "VIP")]
        [ColumnDic(2, "VVIP")]
        [ColumnDic(3, "VVVIP")]
        [Column("c_level")]
        public string Level { get; set; }
    }
}