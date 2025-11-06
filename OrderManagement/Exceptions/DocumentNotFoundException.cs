using System;

namespace OrderManagement.Exceptions
{
    public class DocumentNotFoundException : Exception
    {
        public string DocumentType { get; }
        public string DocumentId { get; }

        public DocumentNotFoundException(string documentType, string documentId)
            : base($"{documentType} with ID {documentId} was not found")
        {
            DocumentType = documentType;
            DocumentId = documentId;
        }

        public DocumentNotFoundException(string documentType, string documentId, Exception innerException)
            : base($"{documentType} with ID {documentId} was not found", innerException)
        {
            DocumentType = documentType;
            DocumentId = documentId;
        }
    }
}