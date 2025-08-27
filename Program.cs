using PdfChat.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Config & services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddHttpClient(); // shared HttpClientFactory

builder.Services.AddSingleton<PdfTextExtractor>();
builder.Services.AddSingleton<TextChunker>();
builder.Services.AddSingleton<OpenAiService>();
builder.Services.AddSingleton<QdrantService>();
builder.Services.AddSingleton<RagService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
