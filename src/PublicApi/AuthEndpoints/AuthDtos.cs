using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.eShopWeb.PublicApi.AuthEndpoints;

public class LoginRequest : BaseRequest
{
    [Required]
    public string UserName { get; set; } = string.Empty; // can be email
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest : BaseRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse : BaseResponse
{
    public LoginResponse(Guid correlationId) : base(correlationId) {}
    public LoginResponse() {}
    public string UserName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
}

public class UserInfoResponse : BaseResponse
{
    public UserInfoResponse(Guid correlationId) : base(correlationId) {}
    public UserInfoResponse() {}
    public string UserName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public bool IsAuthenticated { get; set; }
}
