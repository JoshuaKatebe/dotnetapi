using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase{
    [HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterDto model){
    var user = new User { UserName = model.Email, Email = model.Email, FullName = model.FullName };
    var result = await _userManager.CreateAsync(user, model.Password);

    if (!result.Succeeded) return BadRequest(result.Errors);
    return Ok("User registered successfully");
}

[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginDto model){
    var user = await _userManager.FindByEmailAsync(model.Email);
    if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
        return Unauthorized();

    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes("SuperSecretKey12345");
    var tokenDescriptor = new SecurityTokenDescriptor{
        Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.Id) }),
        Expires = DateTime.UtcNow.AddHours(2),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return Ok(new { token = tokenHandler.WriteToken(token) });
}

[Authorize]
[HttpPut("update")]
public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto model){
    var userId = User.FindFirstValue(ClaimTypes.Name);
    var user = await _userManager.FindByIdAsync(userId);
    if (user == null) return NotFound();

    user.FullName = model.FullName;
    var result = await _userManager.UpdateAsync(user);
    return result.Succeeded ? Ok("User updated") : BadRequest(result.Errors);
}

[Authorize]
[HttpDelete("delete")]
public async Task<IActionResult> DeleteAccount(){
    var userId = User.FindFirstValue(ClaimTypes.Name);
    var user = await _userManager.FindByIdAsync(userId);
    if (user == null) return NotFound();

    var result = await _userManager.DeleteAsync(user);
    return result.Succeeded ? Ok("User deleted") : BadRequest(result.Errors);
}
}
