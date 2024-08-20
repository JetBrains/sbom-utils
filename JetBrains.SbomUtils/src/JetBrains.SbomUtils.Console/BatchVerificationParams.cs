namespace JetBrains.SbomUtils.Console;

record Product(string Name, string RootDirectory, ICollection<string> RootPackages);

record BatchVerificationParams(ICollection<Product> Products, string[] Ignores);
