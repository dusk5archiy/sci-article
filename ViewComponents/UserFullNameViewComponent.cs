using Microsoft.AspNetCore.Mvc;
using SciArticle.Models.Back;
using SciArticle.Models.Object;

namespace SciArticle.ViewComponents
{
    public class UserFullNameViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            string username = HttpContext.User.Identity?.Name ?? string.Empty;
            User? user = await Task.Run(() => UserQuery.GetUserByUsername(username));
            string fullName = user?.Name ?? username;
            return await Task.FromResult<IViewComponentResult>(Content(fullName));
        }
    }
}