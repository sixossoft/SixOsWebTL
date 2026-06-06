using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SixOsTL.MVC.Models;

namespace SixOsTL.MVC.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();   // landing page công khai (●'◡'●)
}
