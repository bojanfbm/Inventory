using Api.Models.Request;
using Api.Validators;
using BusinessLogic.Data;
using BusinessLogic.Models.Exceptions;
using BusinessLogic.Services;
using BusinessLogic.Utility;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IInventoryRepository, InventoryRepository>();
builder.Services.AddSingleton<IInventoryService, InventoryService>();

builder.Services.AddSingleton<IValidator<InventoryCreateRequest>, InventoryCreateRequestValidator>();
builder.Services.AddSingleton<IValidator<ItemCreateRequest>, ItemCreateRequestValidator>();

builder.Services.AddSingleton<ISgtinParser, Sgtin96Parser>();

builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();

builder.Services.AddProblemDetails(opts =>
{
    opts.Map<EntityNotFound>(ex => new ProblemDetails
    {
        Title = nameof(EntityNotFound),
        Detail = ex.Message,
        Status = ex.StatusCode
    });

    opts.Map<InvalidSgtin>(ex => new ProblemDetails
    {
        Title = nameof(InvalidSgtin),
        Detail = ex.Message,
        Status = ex.StatusCode
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => { c.IncludeXmlComments(Path.ChangeExtension(typeof(Program).Assembly.Location, "xml")); }
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
