using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Adiciona o servi�o de controllers.
builder.Services.AddControllers();

// Adiciona servi�o de autentica��o JWT Bearer.
builder.Services.AddAuthentication(options =>
{
    options.DefaultChallengeScheme = "JwtBearer";
    options.DefaultAuthenticateScheme = "JwtBearer";
})

// Define os par�metros de valida��o do Token
.AddJwtBearer("JwtBearer", options => 
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Valida quem est� solicitando 
        ValidateIssuer = true,

        // Valida quem est� recebendo
        ValidateAudience = true,

        // Define se o tempo de expira��o do token ser� validado
        ValidateLifetime = true,

        // Froma de criptografia e ainda valida��o da chave de autentica��o
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("Filmes.chave.autenticacao-webapi-dev")),

        // Valida o tempo de expia��o do token
        ClockSkew = TimeSpan.FromMinutes(5),

        // De onde est� vindo ( issuer )
        ValidIssuer = "webapi.filmes.tarde",

        // Para onde est� indo ( audience )
        ValidAudience = "webapi.filmes.tarde"
    };
});

// Adiciona o gerador de Swagger.
builder.Services.AddSwaggerGen(options =>
{

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "API Filmes",
        Description = "API para gerenciamento de filmes - Introdu��o a sprint 2 - Backend API.",
        Contact = new OpenApiContact
        {
            Name = "Gustavo Magalh�es",
            Url = new Uri("https://github.com/Gustavoozz")
        },
    });

    // Configura o Swagger para usar o arquivo XML.
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    // Usando a autentica��o no Swagger:
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Value: Bearer TokenJWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
        new OpenApiSecurityScheme
           {
            Reference = new OpenApiReference
            {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
            }
        },
        new string[]{}
 }
    });
});

var app = builder.Build();

// Habilite o middleware para atender ao documento JSON gerado e � interface do usu�rio do Swagger.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Para atender � interface do usu�rio do Swagger na raiz do aplicativo.
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

// Usar autentica��o 
app.UseAuthentication();

// Usar autoriza��o
app.UseAuthorization();

//Mapear os controllers.
app.MapControllers();

app.Run();
