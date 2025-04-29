using finalProject.ViewModels;
using finalProject.Models;
namespace finalProject.ViewModels { 

public class UsersViewModel
{
    public List<ApplicationUser> Users { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}
}