using FinScope.ViewModels;
using FinScope.Models;
namespace FinScope.ViewModels { 

public class UsersViewModel
{
    public List<ApplicationUser> Users { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}
}