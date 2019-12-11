dotnet ef dbcontext scaffold `
   name=DefaultDatabase `
   Microsoft.EntityFrameworkCore.SqlServer `
   --output-dir Model `
   --context TopicContext `
   --force `
   --verbose
