dotnet ef dbcontext scaffold `
   name=DatabaseConnectionString `
   Microsoft.EntityFrameworkCore.SqlServer `
   --output-dir Model `
   --context TopicContext `
   --force `
   --verbose
