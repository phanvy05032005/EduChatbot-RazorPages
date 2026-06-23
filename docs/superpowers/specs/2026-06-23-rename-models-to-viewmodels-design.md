# Design Spec: Rename Web Models to ViewModels

Rename the directory `EduChatbot.Web/Models` to `EduChatbot.Web/ViewModels` and update its namespace to resolve model name collisions and follow MVC/Razor Pages best practices.

## Scope

- **Source Directory**: `EduChatbot.Web/Models`
- **Target Directory**: `EduChatbot.Web/ViewModels`
- **Target Namespace**: `EduChatbot.Web.ViewModels`
- **Git Actions**: None (as per user instruction).

## File Migration Plan

We will duplicate the files into the new directory, perform the namespace and usage updates, verify the build, and then delete the old directory.

### 1. New ViewModels Creation
We will create the directory `EduChatbot.Web/ViewModels` and copy the following files:
1. `AccountChangePasswordViewModel.cs`
2. `AccountProfileViewModel.cs`
3. `AdminAccountFormViewModel.cs`
4. `AdminAccountRowViewModel.cs`
5. `AdminRolePermissionViewModel.cs`
6. `ChatMessageViewModel.cs`
7. `DocumentEditViewModel.cs`
8. `LoginViewModel.cs`

### 2. Namespace Updates
For each copied file in `EduChatbot.Web/ViewModels`, change:
```csharp
namespace EduChatbot.Web.Models;
```
to:
```csharp
namespace EduChatbot.Web.ViewModels;
```

### 3. Usage Updates in Pages & Page Models
Update the namespace references in the following files:

- **Razor Imports**:
  - `EduChatbot.Web/Pages/_ViewImports.cshtml`: Change `@using EduChatbot.Web.Models` to `@using EduChatbot.Web.ViewModels`

- **Page Models**:
  - `EduChatbot.Web/Pages/Documents/Edit.cshtml.cs`
  - `EduChatbot.Web/Pages/Chat/Conversation.cshtml.cs`
  - `EduChatbot.Web/Pages/Account/Login.cshtml.cs`
  - `EduChatbot.Web/Pages/Admin/AccountForm.cshtml.cs`
  - `EduChatbot.Web/Pages/Admin/AccountListPageModelBase.cs`
  - `EduChatbot.Web/Pages/Account/Profile.cshtml.cs`
  - `EduChatbot.Web/Pages/Admin/Roles.cshtml.cs`
  - `EduChatbot.Web/Pages/Admin/Users.cshtml.cs`

In each of these `.cshtml.cs` files, replace:
```csharp
using EduChatbot.Web.Models;
```
with:
```csharp
using EduChatbot.Web.ViewModels;
```

## Verification & Cleanup Plan

1. Run `dotnet build` to ensure the project compiles with no errors.
2. Manually test the functionalities: Login, Profile change, Password change, User management, Roles view, Document edit, and Chatbot asking questions.
3. Once verified, delete the old directory `EduChatbot.Web/Models` and run `dotnet build` again to ensure no stray references are left.
