using System.IO;
using Dwragge.RCloneClient.Persistence;
using FluentValidation;

namespace Dwragge.RCloneClient.Common
{
    public class BackupFolderValidator : ValidatorBase<BackupFolderDto>
    {
        protected override void Rules()
        {
            RuleFor(f => f.Path).Must(Directory.Exists).WithMessage("Path must be a directory and exist.");
        }
    }
}