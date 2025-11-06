using System;

namespace OrderManagement.Exceptions
{
    public class DocumentUpdatedFailedException : Exception
    {
        public string DocumentType { get; }
        public string DocumentId { get; }

        public DocumentUpdatedFailedException(string documentType, string documentId)
            : base($"Failed to update {documentType} with ID {documentId}")
        {
            DocumentType = documentType;
            DocumentId = documentId;
        }

        public DocumentUpdatedFailedException(string documentType, string documentId, Exception innerException)
            : base($"Failed to update {documentType} with ID {documentId}", innerException)
        {
            DocumentType = documentType;
            DocumentId = documentId;
        }
    }
}