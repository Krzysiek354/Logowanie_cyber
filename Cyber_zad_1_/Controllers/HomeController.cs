using Cyber_zad_1_.Models;
using Cyber_zad_1_.Models.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

[Authorize]
public class HomeController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly DBContext _context;
    private int minimumLength = 8; // Minimalna długość hasła
    private bool requireUppercase = true; // Czy wymagana wielka litera
    private bool requireDigit = true; // Czy wymagana cyfra
    private bool requireSpecialChar = true; // Czy wymagany znak specjalny
  


    public HomeController(UserManager<User> userManager, SignInManager<User> signInManager, DBContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        
    }


    [HttpPost]
    public IActionResult SetSessionTimeout(int sessionTimeout)
    {
        var passwordRequirements = _context.PasswordRequirements.FirstOrDefault() ?? new PasswordRequirements();

        passwordRequirements.days = sessionTimeout;
        _context.SaveChanges();
        return RedirectToAction("AdminDashboard");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        return View();
    }

   


    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string username, string password)
    {
        var result = await _signInManager.PasswordSignInAsync(username, password, false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByNameAsync(username);
            var passwordRequirements = _context.PasswordRequirements.FirstOrDefault() ?? new PasswordRequirements();
            if ((DateTime.Now - user.PasswordChangedDate).TotalDays > passwordRequirements.days)
            {
                ViewBag.PasswordExpired="PasswordExpired Hasło wygasło. Proszę zmienić hasło.";
                //await _signInManager.SignOutAsync();
                return RedirectToAction("ChangePasswordOnFirstLogin");
            }

            if (user.IsBlocked)
            {
                ViewBag.AccountBlocked = "Konto jest zablokowane.";
                await _signInManager.SignOutAsync();
                return View();
            }

            if (user.IsFirstLogin)
            {
                return RedirectToAction("ChangePasswordOnFirstLogin");
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction("AdminDashboard");
            }

            return RedirectToAction("UserDashboard");
        }

        ViewBag.InvalidCredentials = "Niepoprawny login lub hasło.";
        return View();
    }

    public IActionResult UserDashboard()
    {
        var changePasswordErrors = TempData["ChangePasswordErrors"] as List<string>;

        // Przekaż komunikaty o błędach do widoku
        ViewBag.ChangePasswordErrors = changePasswordErrors;
        return View(); 
    }

    [Route("~/Home/ChangePasswordd")]
    [HttpGet]
    [Authorize(Roles = "User")]
    public IActionResult ChangePasswordd()
    {
        return View();
    }
    [Route("~/Home/ChangePasswordd")]
    [HttpPost]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> ChangePasswordd(string oldPassword, string newPassword)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Home");
        }

        if (user.IsPasswordComplexityEnabled)
        {
            if (IsValidPassword(newPassword))
            {
                _userManager.Options.Password.RequireDigit = false;
                _userManager.Options.Password.RequireLowercase = false;
                _userManager.Options.Password.RequireUppercase = false;
                _userManager.Options.Password.RequireNonAlphanumeric = false;
                _userManager.Options.Password.RequiredLength = 1;
                var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

                if (result.Succeeded)
                {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Login", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            else
            {
                ModelState.AddModelError("", "Hasło nie spełnia wymagań.");
            }
        }
        else
        {
            _userManager.Options.Password.RequireDigit = false;
            _userManager.Options.Password.RequireLowercase = false;
            _userManager.Options.Password.RequireUppercase = false;
            _userManager.Options.Password.RequireNonAlphanumeric = false;
            _userManager.Options.Password.RequiredLength = 1;
            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

            if (result.Succeeded)
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Login", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        // Przekaż komunikaty o błędach jako lista łańcuchów do TempData
        var changePasswordErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        TempData["ChangePasswordErrors"] = changePasswordErrors;

        return RedirectToAction("UserDashboard", "Home");
    }


    [Route("~/Home/AdminDashboard")]
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminDashboard()
    {
        var passwordRequirements = _context.PasswordRequirements.FirstOrDefault() ?? new PasswordRequirements();

        // Przekaż ustawienia do ViewBag
        ViewBag.SessionTimeoutInDays = passwordRequirements.days;

        // Lub ustaw wartość w ViewData
        ViewData["SessionTimeoutInDays"] = passwordRequirements.days;
        ViewBag.MinimumLength = passwordRequirements.MinimumLength;
        ViewBag.RequireUppercase = passwordRequirements.RequireUppercase;
        ViewBag.RequireDigit = passwordRequirements.RequireDigit;
        ViewBag.RequireSpecialChar = passwordRequirements.RequireSpecialChar;
        var users = await _context.Users.ToListAsync();
        return View(users);
    }
    [Route("~/Home/AddUser")]
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult AddUser()
    {
        return View();
    }
    [Route("~/Home/AddUser")]
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddUser(User newUser,string Role)
    {
        var existingUser = await _userManager.FindByNameAsync(newUser.UserName);

        if (existingUser != null)
        {
            ModelState.AddModelError("", "Użytkownik o podanej nazwie już istnieje.");
            return View();
        }

        var user = new User { UserName = newUser.UserName, Email = newUser.Email, PasswordChangedDate = DateTime.Now };
        var result = await _userManager.CreateAsync(user, "InitialAdminPassword123!");

        if (result.Succeeded)
        {
            // Dodawanie użytkownika do roli "User"
            await _userManager.AddToRoleAsync(user, Role);

            return RedirectToAction("AdminDashboard");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View();
    }
    [Route("~/Home/ChangePasswordOnFirstLogin")]
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ChangePasswordOnFirstLogin()
    {
        return View();
    }
    [Route("~/Home/ChangePasswordOnFirstLogin")]
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ChangePasswordOnFirstLogin(string oldPassword, string newPassword)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user.IsPasswordComplexityEnabled)
        {
            if (IsValidPassword(newPassword))
            {

                var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

                if (result.Succeeded)
                {
                    user.IsFirstLogin = false;
                    user.PasswordChangedDate = DateTime.Now;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Login");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            else
            {
                ModelState.AddModelError("", "Hasło nie spełnia wymagań.");
            }
        }
        else
        {
            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

            if (result.Succeeded)
            {
                user.IsFirstLogin = false;
                user.PasswordChangedDate = DateTime.Now;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
        return View();
    }
    [Route("~/Home/EditUser")]
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EditUser(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return View(user);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EditUser(string id,string UserName, string Email)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        user.Email = Email;
        user.UserName = UserName;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return RedirectToAction("AdminDashboard");
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BlockUser(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        user.IsBlocked = !user.IsBlocked;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return RedirectToAction("AdminDashboard");
    }


    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return RedirectToAction("AdminDashboard");
    }


    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user != null)
        {
            var changePasswordResult = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

            if (changePasswordResult.Succeeded)
            {
                // Hasło zostało pomyślnie zmienione
                return RedirectToAction("AdminDashboard");
            }
            else
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
        }
        else
        {
            ModelState.AddModelError("", "Nie znaleziono użytkownika.");
        }

        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EnablePasswordComplexity(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        user.IsPasswordComplexityEnabled = true;
        await _userManager.UpdateAsync(user);

        return RedirectToAction("AdminDashboard");
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DisablePasswordComplexity(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        user.IsPasswordComplexityEnabled = false;
        await _userManager.UpdateAsync(user);

        return RedirectToAction("AdminDashboard");
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult UpdatePasswordRequirements(int minimumLength, int requireUppercase, int requireDigit, int requireSpecialChar)
    {
        // Pobierz lub utwórz rekord PasswordRequirements z bazy danych
        var passwordRequirements = _context.PasswordRequirements.FirstOrDefault() ?? new PasswordRequirements();




        passwordRequirements.Id = 0;
        passwordRequirements.MinimumLength = minimumLength;
        passwordRequirements.RequireUppercase = requireUppercase;
        passwordRequirements.RequireDigit = requireDigit;
        passwordRequirements.RequireSpecialChar = requireSpecialChar;
        _userManager.Options.Password.RequireDigit = false;
        _userManager.Options.Password.RequireLowercase = false;
        _userManager.Options.Password.RequireUppercase = false;
        _userManager.Options.Password.RequireNonAlphanumeric = false;
        _userManager.Options.Password.RequiredLength = 1;

        // Dodaj lub zaktualizuj obiekt w bazie danych
        if (_context.PasswordRequirements.Any(pr => pr.Id == 0))
        {
            _context.PasswordRequirements.Update(passwordRequirements);
        }
        else
        {
            _context.PasswordRequirements.Add(passwordRequirements);
        }

        _context.SaveChanges();

   

        // Przekieruj z powrotem do widoku AdminDashboard lub gdziekolwiek chcesz
        return RedirectToAction("AdminDashboard");
    }

    [Route("~/Home/Logout")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Home");
    }


    private bool IsValidPassword(string password)
    {
        var passwordRequirements = _context.PasswordRequirements.FirstOrDefault() ?? new PasswordRequirements();

        if (password.Length < passwordRequirements.MinimumLength)
        {
            return false;
        }

        int uppercaseCount = 0;
        int digitCount = 0;
        int specialCharCount = 0;

        foreach (char character in password)
        {
            if (char.IsUpper(character))
            {
                uppercaseCount++;
            }
            else if (char.IsDigit(character))
            {
                digitCount++;
            }
            else if ("!@#$%^&*()_-+=".Contains(character))
            {
                specialCharCount++;
            }
        }

       
        if (uppercaseCount >= passwordRequirements.RequireUppercase && digitCount >= passwordRequirements.RequireDigit && specialCharCount >= passwordRequirements.RequireSpecialChar)
        {
            return true;
        }

        return false;
    }
}

