using Dapr.Workflow;
using PizzaWorkflow.Activities;
using PizzaWorkflow.Workflows;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// BUG: The following line is missing from the original code
builder.Services.AddControllers().AddDapr();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDaprWorkflow(options =>
{
    // Register workflows
    options.RegisterWorkflow<PizzaOrderingWorkflow>();

    // Register activities
    options.RegisterActivity<StorefrontActivity>();
    options.RegisterActivity<CookingActivity>();
    options.RegisterActivity<ValidationActivity>();
    options.RegisterActivity<DeliveryActivity>();
});

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();