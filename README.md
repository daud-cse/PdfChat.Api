# PDF Chat API

A powerful .NET 8 Web API that enables intelligent document conversations using RAG (Retrieval-Augmented Generation). Upload PDF documents and chat with them using natural language queries powered by OpenAI embeddings and vector search.

## 🚀 Features

- **PDF Processing**: Extract text from PDF documents using PdfPig library
- **Smart Chunking**: Intelligent text segmentation for optimal retrieval
- **Vector Search**: Fast semantic search using Qdrant vector database
- **AI-Powered Chat**: OpenAI integration for intelligent question answering
- **RAG Pipeline**: Retrieval-Augmented Generation for accurate responses
- **RESTful API**: Clean, documented endpoints with Swagger UI
- **Docker Ready**: Easy deployment with Docker Compose

## 🛠️ Tech Stack

- **Backend**: .NET 8 Web API
- **PDF Processing**: UglyToad.PdfPig
- **Vector Database**: Qdrant
- **AI Services**: OpenAI API
- **Documentation**: Swagger/OpenAPI
- **Containerization**: Docker & Docker Compose

## 📋 Prerequisites

- .NET 8 SDK
- Docker and Docker Compose
- OpenAI API key

## 🚀 Quick Start

### 1. Clone the Repository

```bash
git clone <your-repo-url>
cd PdfChat.Api
```

### 2. Configuration Setup

**⚠️ IMPORTANT: Never commit your actual API keys to version control!**

1. Copy the example configuration file:
   ```bash
   cp appsettings.Example.json appsettings.Development.json
   ```

2. Edit `appsettings.Development.json` and add your actual OpenAI API key:
   ```json
   {
     "OpenAI": {
       "ApiKey": "your-actual-openai-api-key-here"
     }
   }
   ```

3. The `.gitignore` file is configured to exclude all environment-specific configuration files to protect your secrets.

### 3. Start Qdrant Database

```bash
docker-compose up -d
```

This will start Qdrant vector database on port 6333.

### 4. Run the Application

```bash
dotnet run
```

The API will be available at:
- **API**: https://localhost:7000
- **Swagger UI**: https://localhost:7000/swagger

## 📖 How It Works

1. **Upload**: Send PDF files via `/api/upload` endpoint
2. **Process**: Extract text and create semantic chunks
3. **Embed**: Generate vector embeddings using OpenAI
4. **Store**: Index chunks in Qdrant vector database
5. **Query**: Ask questions via `/api/chat` endpoint
6. **Retrieve**: Find relevant document chunks using vector similarity
7. **Generate**: Provide contextual responses based on retrieved content

## 🔌 API Endpoints

### Upload PDF Document
```http
POST /api/upload
Content-Type: multipart/form-data

file: [PDF file]
```

**Response:**
```json
{
  "docId": "unique-document-id",
  "chunkCount": 15
}
```

### Chat with Document
```http
POST /api/chat
Content-Type: application/json

{
  "docId": "your-document-id",
  "question": "What is this document about?"
}
```

**Response:**
```json
{
  "answer": "AI-generated answer based on document content",
  "sources": ["relevant text chunks used for answer"]
}
```

## 🔒 Security & Configuration

### Environment Variables
You can also use environment variables for configuration:

```bash
export OpenAI__ApiKey="your-api-key"
export Qdrant__Host="localhost"
export Qdrant__Port="6333"
```

### Configuration Files
- `appsettings.json` - Base configuration (committed to git)
- `appsettings.Development.json` - Development overrides (gitignored)
- `appsettings.Production.json` - Production overrides (gitignored)

## 🐳 Docker Deployment

### Build and Run
```bash
# Build the application
docker build -t pdfchat-api .

# Run with Docker Compose
docker-compose up -d
```

### Environment Variables in Docker
```bash
docker run -e OpenAI__ApiKey="your-key" -p 7000:7000 pdfchat-api
```

## 📁 Project Structure

```
PdfChat.Api/
├── Controllers/          # API endpoints
├── Models/              # Data models and DTOs
├── Services/            # Business logic services
├── appsettings.json    # Base configuration
├── docker-compose.yml  # Qdrant database setup
└── README.md          # This file
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Troubleshooting

### Common Issues

1. **Qdrant Connection Error**: Ensure Docker is running and Qdrant container is up
2. **OpenAI API Errors**: Verify your API key and account has sufficient credits
3. **PDF Processing Issues**: Check if the PDF file is corrupted or password-protected

### Logs
Check application logs for detailed error information:
```bash
dotnet run --environment Development
```

## 📞 Support

If you encounter any issues or have questions:
1. Check the [Issues](../../issues) page
2. Create a new issue with detailed information
3. Include your configuration (without API keys) and error logs

---

**Remember**: Keep your API keys secure and never commit them to version control! 🔐
