using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Snippet.EntityFrameworkCore.Audit.Sample.Data;
using System;

namespace Snippet.EntityFrameworkCore.Audit.Sample.Pages
{
    public class AddModel : PageModel
    {
        public void OnGet([FromServices] SampleDbContext sampleDbContext)
        {
            var random = new Random(DateTime.Now.Millisecond);
            sampleDbContext.TestEntities.Add(new SampleUser
            {
                Name = random.Next(1, 100).ToString(),
                Account = random.Next(1, 1000000).ToString(),
                Level = random.Next(1, 3).ToString()
            });
            sampleDbContext.SaveChanges();
        }
    }
}