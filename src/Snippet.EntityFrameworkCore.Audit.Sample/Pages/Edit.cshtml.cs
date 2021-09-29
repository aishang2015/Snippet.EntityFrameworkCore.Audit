using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Snippet.EntityFrameworkCore.Audit.Sample.Data;
using System;
using System.Linq;

namespace Snippet.EntityFrameworkCore.Audit.Sample.Pages
{
    public class EditModel : PageModel
    {
        public void OnGet([FromServices] SampleDbContext sampleDbContext)
        {
            using var tran = sampleDbContext.Database.BeginTransaction();

            var entities = sampleDbContext.TestEntities.Take(5);
            var random = new Random(DateTime.Now.Millisecond);
            foreach (var entity in entities)
            {
                entity.Account = random.Next(1, 1000000).ToString();
            }
            sampleDbContext.SaveChanges();

            sampleDbContext.TestEntities.Add(new SampleUser
            {
                Name = random.Next(1, 100).ToString(),
                Account = random.Next(1, 1000000).ToString(),
            });
            sampleDbContext.SaveChanges();

            tran.Commit();
        }
    }
}