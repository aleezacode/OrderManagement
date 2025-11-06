using System;

namespace OrderManagement.Exceptions
{
    public class DocumentCreationFailedException : Exception
    {
        public string DocumentType { get; }

        public DocumentCreationFailedException(string documentType)
            : base($"Failed to create {documentType}")
        {
            DocumentType = documentType;
        }

        public DocumentCreationFailedException(string documentType, Exception innerException)
            : base($"Failed to create {documentType}", innerException)
        {
            DocumentType = documentType;
        }
    }
}