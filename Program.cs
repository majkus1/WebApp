using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
        httpsOptions.ClientCertificateValidation = (certificate, chain, errors) =>
        {
            if (certificate == null)
            {
                return false;
            }
            return certificate.Subject.Contains("CN=userml");
        };
    });
});

var app = builder.Build();

if (app.Environment.IsProduction())
{
    app.Use(async (context, next) =>
    {
        if (context.Request.Headers.TryGetValue("X-ARR-ClientCert", out var certHeader))
{
    var certHeaderString = certHeader.ToString();
    if (string.IsNullOrEmpty(certHeaderString))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Brak certyfikatu");
        return;
    }
    
    var certBytes = Convert.FromBase64String(certHeaderString);
    var certificate = new X509Certificate2(certBytes);
    
    if (!certificate.Subject.Contains("CN=userml"))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync("Nieodpowiedni certyfikat");
        return;
    }
}
else
{
    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    await context.Response.WriteAsync("Brak certyfikatu");
    return;
}
        await next();
    });
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();
 