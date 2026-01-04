# Paralax.Auth

**Paralax.Auth** is a lightweight, extensible authentication module for the **Paralax microservices framework**, providing JWT-based authentication, token validation, and optional authentication bypass for development and internal scenarios.

It is designed for **modern .NET microservices**, with first-class support for **ASP.NET Core**, **JWT Bearer authentication**, and **scalable distributed architectures**.

---

## ✨ Features

- 🔐 JWT authentication (HMAC or X.509 certificate based)
- 🧩 Seamless integration with the Paralax framework
- 🏗️ Clean extension-based configuration
- 🛂 Token issuance and validation
- 🚫 Token revocation (in-memory blacklist)
- 🔓 Optional authentication disabling (development/testing)
- ⚡ Minimal overhead, production-ready defaults
- 🧠 Strongly typed configuration options

---

## 📦 Installation

Install from NuGet:

```bash
dotnet add package Paralax.Auth
````

---

## 🚀 Quick Start

### 1️⃣ Register authentication

```csharp
builder.AddParalax()
       .AddJwt();
```

By default, configuration is read from the `jwt` section in your configuration files.

---

### 2️⃣ Enable middleware

```csharp
app.UseAuthentication();
app.UseAuthorization();
app.UseAccessTokenValidator();
```

---

## ⚙️ Configuration

### Basic symmetric key configuration (recommended for most services)

```json
"jwt": {
  "issuer": "paralax-auth",
  "issuerSigningKey": "very_secure_secret_key_123456",
  "expiryMinutes": 60,
  "validateIssuer": true,
  "validateAudience": false
}
```

---

### X.509 certificate configuration (recommended for high-security environments)

```json
"jwt": {
  "issuer": "paralax-auth",
  "certificate": {
    "location": "certs/jwt-signing.pfx",
    "password": "cert-password"
  },
  "expiryMinutes": 60
}
```

You may also provide the certificate as Base64 (`rawData`) instead of a file path.

---

## 🧩 JwtOptions Overview

| Option                     | Description                                |
| -------------------------- | ------------------------------------------ |
| `Issuer`                   | Token issuer                               |
| `ValidIssuer(s)`           | Allowed token issuers                      |
| `IssuerSigningKey`         | Symmetric signing key                      |
| `Certificate`              | X.509 certificate for signing              |
| `Algorithm`                | Security algorithm (default auto-selected) |
| `Expiry` / `ExpiryMinutes` | Token lifetime                             |
| `ValidateLifetime`         | Enable expiration validation               |
| `ValidateAudience`         | Enable audience validation                 |
| `ValidAudience(s)`         | Allowed audiences                          |
| `AuthenticationDisabled`   | Disable authentication entirely            |
| `AllowAnonymousEndpoints`  | Paths excluded from validation             |

---

## 🔐 Issuing Tokens

Inject `IJwtHandler` and generate tokens programmatically:

```csharp
public class AuthService
{
    private readonly IJwtHandler _jwtHandler;

    public AuthService(IJwtHandler jwtHandler)
    {
        _jwtHandler = jwtHandler;
    }

    public string CreateToken(string userId)
    {
        return _jwtHandler.CreateToken(userId).AccessToken;
    }
}
```

Supports:

* User ID (subject)
* Role
* Audience
* Custom claims

---

## 🛡️ Token Validation & Revocation

### Automatic validation

JWT validation is handled by ASP.NET Core authentication middleware.

### Token revocation (logout support)

Paralax.Auth includes an **in-memory token blacklist**:

```csharp
await accessTokenService.DeactivateCurrentAsync();
```

Once revoked, the token becomes invalid until it expires.

> ⚠️ In-memory storage is per-instance.
> For distributed systems, use a shared cache (e.g. Redis) via a custom `IAccessTokenService`.

---

## 🔓 Disable Authentication (Development / Testing)

```json
"jwt": {
  "authenticationDisabled": true
}
```

This bypasses authentication entirely while preserving the request pipeline.

⚠️ **Never enable this in production.**

---

## 🧪 Anonymous Endpoints

Exclude selected paths from token validation:

```json
"jwt": {
  "allowAnonymousEndpoints": [
    "/health",
    "/metrics",
    "/swagger"
  ]
}
```

---

## 🧱 Architecture Overview

* **Extensions** – Fluent configuration via `AddJwt`
* **JwtHandler** – Token creation and parsing
* **AccessTokenValidatorMiddleware** – Runtime validation
* **IAccessTokenService** – Token revocation abstraction
* **DisabledAuthenticationPolicyEvaluator** – Authentication bypass

The design follows **SOLID principles** and is fully extensible.

---

## 🔄 Framework Compatibility

| .NET Version |
| ------------ |
| .NET 8.0     |
| .NET 9.0     |

---

## 📄 License

Licensed under the **Apache License 2.0**.

See [LICENSE](https://www.apache.org/licenses/LICENSE-2.0).

---

## 🧠 When to Use Paralax.Auth

✅ Microservices
✅ Internal APIs
✅ Gateway authentication
✅ Stateless JWT authentication
✅ Paralax-based systems

---

## 🤝 Contributing

Contributions are welcome.

* Fork the repository
* Create a feature branch
* Submit a pull request

Repository:
👉 [https://github.com/ITSharpPro/Paralax](https://github.com/ITSharpPro/Paralax)

---

## 🏢 Authors & Maintainers

**ITSharpPro**
🌐 [https://itsharppro.com](https://itsharppro.com)

**Andrii Voznesenskyi**
GitHub: [https://github.com/SaintAngeLs](https://github.com/SaintAngeLs)

---

## ⭐ Final Notes

Paralax.Auth is intentionally **simple, explicit, and predictable**.
It provides **full control over authentication** without unnecessary abstractions.

If you need:

* Distributed token revocation
* External identity providers
* Advanced claim mapping

You can extend it cleanly without breaking the core.

---

**Happy building 🚀**

