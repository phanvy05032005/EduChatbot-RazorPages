# EduChatbot - Hướng dẫn chạy project

Tài liệu này hướng dẫn chi tiết cách cài đặt môi trường, cấu hình dịch vụ, thiết lập cơ sở dữ liệu và chạy ứng dụng **EduChatbot** dành cho người mới bắt đầu.

---

## 1. Giới thiệu nhanh

**EduChatbot** là một ứng dụng học tập thông minh dựa trên mô hình ngôn ngữ lớn (LLM) hỗ trợ sinh viên học tập thông qua tài liệu môn học được tải lên bởi giảng viên:
* **Quản lý tài liệu**: Giảng viên (Lecturer) có thể tải lên tài liệu học tập (PDF, DOCX). Hệ thống tự động phân tách tài liệu thành các đoạn (chunks) và tạo vector embedding để phục vụ tìm kiếm ngữ cảnh.
* **Chatbot học thuật**: Sinh viên (Student) có thể trò chuyện với chatbot AI về nội dung tài liệu học tập của từng môn học cụ thể, có hiển thị trích dẫn nguồn rõ ràng.
* **Hạn mức lượt hỏi (Quota)**: Hệ thống giới hạn số lượt hỏi (Request Count) của tài khoản tùy theo gói dịch vụ:
  * **Basic** (Miễn phí): Cho phép Chat AI với hạn mức tối đa 20 lượt hỏi/ngày, tính năng Quiz bị khóa.
  * **Premium** (Trả phí): Cho phép Chat AI với hạn mức tối đa 100 lượt hỏi/ngày, mở khóa tính năng Quiz.
* **Thanh toán nâng cấp**: Tích hợp cổng thanh toán **PayOS** để nâng cấp tài khoản từ Basic lên Premium một cách nhanh chóng và tự động cập nhật trạng thái thông qua webhook/callback.
* **Hệ thống gửi thư (Email Queue)**: Gửi thông tin tài khoản tự động tới email sinh viên hoặc giảng viên sau khi tài khoản được tạo bởi Admin thông qua hàng đợi gửi nền (Background Queue).
* **Quản trị hệ thống (Admin Dashboard)**: Giúp Admin quản lý tài khoản sinh viên/giảng viên, phân công môn học, kiểm duyệt tài liệu tải lên và theo dõi thống kê hệ thống.

---

## 2. Công nghệ sử dụng

* **Môi trường & Ngôn ngữ**: .NET 9 (C#, ASP.NET Core Razor Pages)
* **ORM & Database**: Entity Framework Core (EF Core) kết hợp với PostgreSQL + pgvector (để lưu trữ dữ liệu vector phục vụ tìm kiếm ngữ nghĩa)
* **Ảo hóa**: Docker & Docker Compose (cho việc chạy database nhanh chóng)
* **Trí tuệ nhân tạo (AI)**: 
  * OpenRouter API (sử dụng cho việc Chatbot AI sinh câu trả lời)
  * Vector Embedding API (sử dụng mô hình embedding để tìm kiếm thông tin tương đồng)
* **Cổng thanh toán**: PayOS API
* **Thư điện tử**: SMTP Server (Gmail SMTP hoặc các nhà cung cấp dịch vụ SMTP khác)

---

## 3. Cấu trúc thư mục

Hệ thống được thiết kế theo mô hình phân lớp rõ ràng nhằm giảm thiểu sự phụ thuộc lẫn nhau:
* [EduChatbot.Web](file:///e:/FPTU/SU26/PRN222/ASM/EduChatbot-RazorPages-main/EduChatbot.Web): Dự án giao diện người dùng (Razor Pages), cấu hình khởi chạy (`Program.cs`, `appsettings.json`, các Assets tĩnh).
* [EduChatbot.Business](file:///e:/FPTU/SU26/PRN222/ASM/EduChatbot-RazorPages-main/EduChatbot.Business): Chứa toàn bộ các xử lý logic nghiệp vụ như Chat, phân tích tài liệu, xử lý thanh toán PayOS, đối soát gói cước Subscription, Seeder, DTOs và ViewModels.
* [EduChatbot.Data](file:///e:/FPTU/SU26/PRN222/ASM/EduChatbot-RazorPages-main/EduChatbot.Data): Chứa `ApplicationDbContext`, cấu hình thực thể EF Core và các Migration tương ứng.
* [EduChatbot.Models](file:///e:/FPTU/SU26/PRN222/ASM/EduChatbot-RazorPages-main/EduChatbot.Models): Định nghĩa các thực thể dữ liệu (Entities), Enums, và lớp danh tính người dùng (`ApplicationUser`, `ApplicationRoles`).

---

## 4. Yêu cầu cài đặt trước

Trước khi bắt đầu, hãy đảm bảo máy tính của bạn đã cài đặt các phần mềm sau:
1. **.NET 9 SDK**: Tải và cài đặt từ trang chủ Microsoft.
2. **Docker Desktop**: Cần thiết để khởi chạy PostgreSQL với extension pgvector nhanh chóng mà không cần cài đặt PostgreSQL thủ công.
3. **Git**: Phục vụ việc nhân bản (clone) mã nguồn.
4. **EF Core CLI Tools**: Công cụ dòng lệnh của EF Core để chạy database migration.
   * Cài đặt mới:
     ```bash
     dotnet tool install --global dotnet-ef
     ```
   * Cập nhật lên bản mới nhất (nếu đã cài đặt trước đó):
     ```bash
     dotnet tool update --global dotnet-ef
     ```
5. *(Tùy chọn)* **PostgreSQL Client**: Ví dụ DBeaver hoặc pgAdmin để xem và truy vấn trực tiếp vào cơ sở dữ liệu.

---

## 5. Clone project và restore package

Mở terminal tại thư mục bạn muốn lưu dự án và chạy các lệnh sau:
```bash
git clone <REPOSITORY_URL>
cd EduChatbot-RazorPages-main
dotnet restore
```

---

## 6. Cấu hình biến môi trường / secrets

Để bảo mật thông tin nhạy cảm (API Keys, Mật khẩu cơ sở dữ liệu, Cổng thanh toán), **tuyệt đối không commit các khóa thật** lên Git. Bạn có thể chọn cấu hình thông qua 1 trong 2 cách sau:

### Cách A: Sử dụng User Secrets (Khuyên dùng cho phát triển cục bộ)
User Secrets lưu trữ thông tin ngoài thư mục dự án, tránh việc vô tình commit lên Git.
Chạy lệnh khởi tạo tại thư mục dự án `EduChatbot.Web`:
```bash
cd EduChatbot.Web
dotnet user-secrets init
```

Sau đó, cấu hình các khóa cần thiết bằng lệnh dưới đây (thay thế các giá trị `YOUR_*` bằng thông tin thật của bạn):
```bash
# Thiết lập cấu hình OpenRouter AI
dotnet user-secrets set "OpenRouter:ApiKey" "YOUR_OPENROUTER_API_KEY"
dotnet user-secrets set "Embedding:ApiKey" "YOUR_OPENROUTER_API_KEY"

# Thiết lập cổng thanh toán PayOS
dotnet user-secrets set "PayOS:ClientId" "YOUR_PAYOS_CLIENT_ID"
dotnet user-secrets set "PayOS:ApiKey" "YOUR_PAYOS_API_KEY"
dotnet user-secrets set "PayOS:ChecksumKey" "YOUR_PAYOS_CHECKSUM_KEY"
dotnet user-secrets set "PayOS:WebhookUrl" "https://YOUR_NGROK_DOMAIN.ngrok-free.app/api/payment/payos/webhook"

# Thiết lập dịch vụ SMTP gửi Email (ví dụ dùng Gmail App Password)
dotnet user-secrets set "Smtp:Username" "YOUR_SMTP_USERNAME"
dotnet user-secrets set "Smtp:Password" "YOUR_SMTP_PASSWORD"
```

### Cách B: Sử dụng file `appsettings.Development.json`
Tạo file [appsettings.Development.json](file:///e:/FPTU/SU26/PRN222/ASM/EduChatbot-RazorPages-main/EduChatbot.Web/appsettings.Development.json) tại thư mục `EduChatbot.Web` với nội dung mẫu bên dưới. *Lưu ý: Hãy đưa file này vào danh sách `.gitignore` nếu bạn cập nhật thông tin thật vào đây.*

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=educhatbotdb;Username=postgres;Password=YOUR_POSTGRES_PASSWORD"
  },
  "OpenRouter": {
    "ApiKey": "YOUR_OPENROUTER_API_KEY",
    "Model": "nvidia/nemotron-3-super-120b-a12b:free",
    "BaseUrl": "https://openrouter.ai/api/v1/chat/completions"
  },
  "Embedding": {
    "ApiKey": "YOUR_OPENROUTER_API_KEY",
    "Model": "openai/text-embedding-3-small",
    "BaseUrl": "https://openrouter.ai/api/v1/embeddings",
    "Dimensions": 1536
  },
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "EnableSsl": true,
    "Username": "YOUR_SMTP_USERNAME",
    "Password": "YOUR_SMTP_PASSWORD",
    "SenderName": "EduChatbot System",
    "SenderEmail": "YOUR_SMTP_SENDER_EMAIL"
  },
  "PayOS": {
    "ClientId": "YOUR_PAYOS_CLIENT_ID",
    "ApiKey": "YOUR_PAYOS_API_KEY",
    "ChecksumKey": "YOUR_PAYOS_CHECKSUM_KEY",
    "WebhookUrl": "https://YOUR_NGROK_DOMAIN.ngrok-free.app/api/payment/payos/webhook",
    "AutoConfirmWebhook": true
  }
}
```

---

## 7. Cấu hình Docker / PostgreSQL (pgvector)

Dự án sử dụng cơ sở dữ liệu PostgreSQL kèm theo module mở rộng `pgvector` chạy trên cổng **5433** (xem định nghĩa ở `docker-compose.yml`).

1. Tạo file `.env` nằm cạnh file `docker-compose.yml` ở thư mục gốc của dự án:
   ```env
   POSTGRES_PASSWORD=YOUR_POSTGRES_PASSWORD
   ```
2. Khởi chạy cơ sở dữ liệu thông qua Docker Compose:
   ```bash
   docker compose up -d
   ```
3. Kiểm tra xem container đã khởi chạy thành công hay chưa:
   ```bash
   docker ps
   ```
   Bạn sẽ thấy container `educhatbot-postgres` chạy trên cổng `0.0.0.0:5433->5432/tcp`.

---

## 8. Cấu hình Connection String

Chuỗi kết nối mặc định của hệ thống được định nghĩa trong [appsettings.json](file:///e:/FPTU/SU26/PRN222/ASM/EduChatbot-RazorPages-main/EduChatbot.Web/appsettings.json):
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5433;Database=educhatbotdb;Username=postgres;Password=YOUR_POSTGRES_PASSWORD"
}
```
*Lưu ý: Port mặc định của container PostgreSQL trong docker-compose.yml là **5433**.*

---

## 9. Chạy Migration EF Core

Khi cơ sở dữ liệu trong Docker đã chạy, hãy tiến hành khởi tạo cấu trúc bảng dữ liệu:

1. Đứng ở thư mục gốc (Root Project) và chạy lệnh:
   ```bash
   dotnet ef database update --project EduChatbot.Data --startup-project EduChatbot.Web
   ```
2. Nếu bạn đang đứng tại thư mục dự án `EduChatbot.Web`, hãy sử dụng lệnh sau:
   ```bash
   dotnet ef database update --project ../EduChatbot.Data --startup-project .
   ```

---

## 10. Chạy ứng dụng

1. Đứng ở thư mục gốc và khởi chạy dự án:
   ```bash
   dotnet run --project EduChatbot.Web
   ```
2. Trên màn hình Terminal sẽ hiển thị địa chỉ chạy local:
   ```text
   info: Microsoft.Hosting.Lifetime[14]
         Now listening on: http://localhost:5287
   ```
3. Mở trình duyệt web của bạn và truy cập địa chỉ: `http://localhost:5287`.

---

## 11. Cấu hình ngrok cho PayOS Webhook

Để PayOS gửi thông tin giao dịch thành công (Webhook) về máy tính cá nhân của bạn, bạn cần tạo một đường hầm mạng công khai (Public Tunnel) thông qua **ngrok**:

1. Cài đặt ngrok và khởi chạy lệnh ánh xạ cổng chạy web app (mặc định cổng `5287`):
   ```bash
   ngrok http 5287
   ```
2. Copy URL có dạng `https://xxxx.ngrok-free.app` được sinh ra bởi ngrok.
3. Thiết lập biến cấu hình WebhookUrl:
   * Đường dẫn Webhook có cấu trúc: `https://xxxx.ngrok-free.app/api/payment/payos/webhook`.
   * Cập nhật vào User Secrets hoặc cấu hình của bạn:
     ```bash
     dotnet user-secrets set "PayOS:WebhookUrl" "https://xxxx.ngrok-free.app/api/payment/payos/webhook"
     ```
4. **Lưu ý quan trọng**: 
   * Tài khoản ngrok miễn phí sẽ thay đổi URL ngẫu nhiên mỗi lần bạn khởi động lại lệnh `ngrok`. Do đó, bạn cần phải cập nhật lại cấu hình `WebhookUrl` và khởi động lại ứng dụng nếu khởi chạy lại ngrok.
   * Hãy chắc chắn không có khoảng trắng hoặc ký tự lạ trước chuỗi `https://`.

---

## 12. Kịch bản kiểm thử (Test Flows)

Sau khi khởi chạy ứng dụng thành công, bạn có thể thực hiện kiểm thử các luồng chức năng cốt lõi sau:

### 12.1 Đăng nhập tài khoản mẫu
* Hệ thống sẽ tự động gieo mầm dữ liệu (Seed) một tài khoản quản trị viên mặc định:
  * **Email**: `admin@educhatbot.local`
  * **Mật khẩu**: `Admin@123456`
* Sau khi đăng nhập bằng tài khoản Admin, bạn có thể truy cập `/Admin/Students` hoặc `/Admin/Lecturers` để tạo thêm các tài khoản Sinh viên/Giảng viên để thử nghiệm các luồng khác.

### 12.2 Kiểm thử gói dịch vụ mặc định (Basic Subscription)
* Đăng nhập bằng tài khoản Sinh viên (Student).
* Truy cập trang gói dịch vụ cá nhân tại đường dẫn: `/Subscription/Me`.
* **Kỳ vọng**: 
  * Gói hiện tại hiển thị tên là `Basic`.
  * Hạn mức hiển thị là 20 lượt hỏi.
  * Trạng thái tính năng Quiz hiển thị: `Bị khóa — Cần nâng cấp Premium`.

### 12.3 Kiểm thử giới hạn lượt hỏi Chat AI
* Truy cập trang chat `/Chat/Index` (hoặc tạo một phiên trò chuyện mới).
* Thực hiện gửi một câu hỏi (ví dụ: "ASP.NET Core Razor Pages là gì?").
* Quay lại trang `/Subscription/Me` hoặc xem trực tiếp trên trang `/Subscription/Plans`.
* **Kỳ vọng**: Số lượt hỏi còn lại sẽ giảm từ `20` lượt xuống còn `19` lượt sau khi nhận được câu trả lời từ AI thành công.

### 12.4 Kiểm thử nâng cấp gói Premium (PayOS)
* Truy cập trang danh sách gói dịch vụ tại `/Subscription/Plans`.
* Tại thẻ **Premium**, bấm nút **Nâng cấp Premium**.
* Hệ thống sẽ tạo một yêu cầu thanh toán trên PayOS và tự động chuyển hướng bạn đến trang thanh toán của PayOS.
* Sau khi hoàn tất thanh toán giả lập hoặc thật thành công:
  * PayOS sẽ chuyển hướng bạn về lại đường dẫn `/Subscription/Callback`.
  * Ứng dụng tự động xử lý và đồng bộ trạng thái giao dịch: Chuyển hướng người dùng về `/Subscription/Me` kèm thông báo nâng cấp thành công.
  * Gói Premium chuyển trạng thái sang `ACTIVE`, đồng thời gói Basic chuyển sang `SUSPENDED`.
  * Số lượt hỏi khả dụng hiển thị là `100` lượt và tính năng làm Quiz sẽ hiển thị: `Đã mở khóa`.

### 12.5 Xử lý giao dịch chờ giả lập (Pending Payment Bypass) & Hết hạn liên kết
* Khi bạn bấm nâng cấp Premium nhưng không thanh toán mà quay lại ứng dụng, hệ thống sẽ báo: *"Bạn đang có một giao dịch Premium chờ xử lý"*.
* Để hỗ trợ kiểm thử nhanh, liên kết thanh toán PayOS được cấu hình thời gian hết hạn là **90 giây** kể từ lúc tạo. Nếu sau 90 giây người dùng không hoàn tất thanh toán, liên kết sẽ tự động hết hạn (Expired), trạng thái giao dịch chuyển thành `CANCELLED` và hệ thống tự động giải phóng để cho phép tạo giao dịch nâng cấp mới. Đồng thời, khi truy cập liên kết đã hết hạn, người dùng sẽ tự động chuyển hướng quay lại màn hình danh sách gói `/Subscription/Plans`.
* Ngoài ra, để reset nhanh phục vụ demo mà không cần đợi liên kết hết hạn, bạn có thể thực hiện chạy câu lệnh SQL này trong database:
  ```sql
  UPDATE payment_transactions
  SET created_at = NOW() - INTERVAL '20 minutes'
  WHERE status = 'PENDING';
  ```
  Sau khi chạy, giao dịch cũ sẽ được coi là đã quá hạn (stale) và hệ thống sẽ cho phép tạo giao dịch nâng cấp mới bình thường.

---

## 13. Các Route / Trang chính trong hệ thống

### Dành cho Sinh viên (Student)
* `/`: Trang chủ.
* `/Account/Login`: Trang đăng nhập.
* `/Chat/Index`: Trang danh sách và giao diện chat chính.
* `/Chat/Conversation?id={id}`: Trang chi tiết cuộc trò chuyện.
* `/Subscription/Plans`: Xem danh sách các gói dịch vụ (Basic & Premium) và thao tác nâng cấp.
* `/Subscription/Me`: Xem chi tiết gói hiện dùng, số lượt còn lại và biểu đồ sử dụng.
* `/Subscription/Callback`: Nhận kết quả phản hồi từ PayOS sau khi thanh toán.

### Dành cho Giảng viên (Lecturer)
* `/Documents/Dashboard`: Trang tổng quan tài liệu đã tải lên.
* `/Documents/Index`: Danh sách các tài liệu hiện có và trạng thái kiểm duyệt.
* `/Documents/Upload`: Trang tải tài liệu mới lên hệ thống.

### Dành cho Quản trị viên (Admin)
* `/Admin/Dashboard`: Bảng điều khiển tổng quan hệ thống.
* `/Admin/Students`: Quản lý tài khoản sinh viên.
* `/Admin/Lecturers`: Quản lý tài khoản giảng viên.
* `/Admin/Courses`: Quản lý danh sách môn học và phân công giảng viên phụ trách.
* `/Admin/PendingDocuments`: Kiểm duyệt tài liệu có điểm đánh giá mức độ tương thích môn học thấp.

---

## 14. Các API Endpoint quan trọng

* **`POST /api/payment/payos/webhook`**
  * **Mục đích**: Nhận thông tin thông báo thanh toán (IPN Webhook) gửi từ cổng PayOS.
  * **Xác thực**: Không yêu cầu đăng nhập (Anonymous), tuy nhiên hệ thống sẽ tự động xác thực chữ ký (Signature) đính kèm trong payload để chống giả mạo.

---

## 15. Quy trình trạng thái (Status Lifecycles)

### Trạng thái giao dịch (PaymentTransaction)
* `PENDING`: Đang chờ người dùng quét mã thanh toán PayOS.
* `SUCCESS`: Thanh toán thành công, được đồng bộ qua callback hoặc webhook.
* `FAILED`: Giao dịch thất bại hoặc quá hạn 15 phút không thanh toán.
* `CANCELLED`: Người dùng nhấn nút hủy thanh toán tại trang thanh toán của PayOS.

### Trạng thái gói dịch vụ (SubscriptionStatus)
* `PENDING`: Gói Premium đang chờ xử lý thanh toán.
* `ACTIVE`: Gói dịch vụ hiện đang được sử dụng để tính lượt hỏi.
* `SUSPENDED`: Gói Basic tạm ngưng hoạt động khi gói Premium được kích hoạt thành công.
* `EXPIRED`: Gói Premium đã hết hạn sử dụng. Khi xảy ra, hệ thống tự động kích hoạt lại gói Basic về `ACTIVE`.
* `CANCELLED`: Giao dịch thanh toán Premium bị hủy/thất bại, bản ghi Premium tương ứng sẽ bị hủy.

---

## 16. Bảng gỡ lỗi (Troubleshooting)

| Lỗi | Nguyên nhân | Cách xử lý |
| --- | --- | --- |
| **Docker daemon not running** | Ứng dụng Docker Desktop chưa được bật trên máy tính. | Mở Docker Desktop và đợi đến khi biểu tượng Docker chuyển sang màu xanh lá cây, sau đó chạy lại lệnh compose. |
| **POSTGRES_PASSWORD variable is not set** | Thiếu file `.env` hoặc chưa khai báo biến môi trường này cạnh file `docker-compose.yml`. | Tạo file `.env` cạnh `docker-compose.yml` và khai báo dòng `POSTGRES_PASSWORD=YOUR_PASSWORD` bên trong. |
| **Project file does not exist** | Đứng sai thư mục khi chạy câu lệnh Migration EF Core. | Đảm bảo bạn đứng đúng thư mục chứa file `.sln`. Hoặc chỉ định rõ đường dẫn chính xác tới dự án thông qua tham số `--project`. |
| **relation subscription_plans does not exist** | Bạn đã khởi chạy ứng dụng nhưng chưa thực hiện chạy Migration cơ sở dữ liệu. | Chạy lệnh `dotnet ef database update --project EduChatbot.Data --startup-project EduChatbot.Web` để khởi tạo cấu trúc bảng. |
| **PayOS webhook confirm BadRequest** | Webhook URL được đăng ký với PayOS không thể truy cập từ môi trường bên ngoài (ví dụ dùng localhost không qua ngrok). | Khởi chạy ngrok và cập nhật đúng URL `https` công khai của ngrok vào cấu hình `PayOS:WebhookUrl`. |
| **Không tìm thấy giao dịch** | Webhook PayOS gửi thông tin mã đơn hàng (`orderCode`) không tồn tại trong hệ thống cục bộ. | Kiểm tra xem cơ sở dữ liệu local có bị xóa hoặc reset trong quá trình kiểm thử hay không. Hệ thống sẽ tự động bỏ qua giao dịch lạ. |
| **Bạn đang có một giao dịch Premium chờ xử lý** | Đang tồn tại một hóa đơn Premium có trạng thái `PENDING` dưới 15 phút. | Sử dụng công cụ Database SQL chạy câu lệnh chuyển trạng thái giao dịch cũ về quá hạn (xem mục 12.5) hoặc đợi đủ 15 phút. |
| **OpenRouter API key missing** | Chưa thiết lập khóa API của OpenRouter hoặc khóa bị hết hạn, hết tiền. | Đăng ký tài khoản OpenRouter, tạo API key và cấu hình chính xác vào User Secrets dưới trường `OpenRouter:ApiKey`. |
| **Chat không trừ lượt hỏi** | Múi giờ hệ thống database bị sai hoặc so sánh múi giờ Local và UTC bị lệch dẫn đến chu kỳ tự reset lượt hỏi bị kích hoạt liên tục. | Đảm bảo trường `RequestWindowStart` của gói dịch vụ sử dụng chuẩn UTC trong C#. Mã nguồn hiện tại đã đồng bộ chuẩn hóa múi giờ UTC. |
| **Ngrok 502 Bad Gateway** | Ngrok đang chạy nhưng ứng dụng Web `.NET` cục bộ chưa được khởi chạy hoặc chạy sai cổng. | Đảm bảo bạn đã chạy lệnh `dotnet run --project EduChatbot.Web` và ứng dụng đang lắng nghe đúng cổng chỉ định trên ngrok. |

---

## 17. Hướng dẫn khởi chạy nhanh cho Demo (Copy-Paste)

Nhấp và chạy nhanh các câu lệnh sau để chuẩn bị môi trường chạy demo:

**Terminal 1 (Chạy DB và Ứng dụng Web):**
```bash
# 1. Tạo file biến môi trường cho Database
echo "POSTGRES_PASSWORD=123456" > .env

# 2. Khởi động PostgreSQL container
docker compose up -d

# 3. Khởi tạo database
dotnet ef database update --project EduChatbot.Data --startup-project EduChatbot.Web

# 4. Khởi chạy Web app
dotnet run --project EduChatbot.Web
```

**Terminal 2 (Mở kết nối ngoài cho PayOS Webhook):**
```bash
# Ánh xạ cổng local ra internet công khai
ngrok http 5287
```

*Nhớ copy địa chỉ HTTPS ngrok cung cấp để cấu hình lại `"PayOS:WebhookUrl"` trong User Secrets hoặc appsettings.*

---

## 18. Lưu ý bảo mật

* **Tuyệt đối không commit** file `appsettings.json` hoặc bất kỳ file cấu hình nào lên GitHub nếu có chứa thông tin API Key thật hoặc Mật khẩu SMTP thực tế.
* Khuyến khích sử dụng công cụ **User Secrets** trong quá trình lập trình local để tránh lộ thông tin.
* Trong trường hợp vô tình làm lộ API Key, hãy truy cập trang quản trị của nhà cung cấp dịch vụ (OpenRouter, PayOS) để thu hồi (Revoke) ngay lập tức và tạo khóa mới thay thế.

---

## 19. Ghi chú đặc biệt cho buổi chấm Demo

* Cơ chế đếm số lượt hỏi sử dụng **đếm số lượt hỏi thành công** (Request Count) thay vì đếm số token để giúp buổi demo trực quan và dễ theo dõi.
* Hạn mức mặc định: Gói **Basic** có **20 lượt hỏi/ngày**, gói **Premium** có **100 lượt hỏi/ngày** và mở khóa chức năng tự động sinh Quiz kiểm tra kiến thức.
* Việc cập nhật gói Premium hoạt động hoàn toàn tự động thông qua PayOS Webhook hoặc Callback URL. Bạn có thể sử dụng mã đơn thanh toán giả lập của PayOS trong môi trường Sandbox để demo quy trình thanh toán thành công mà không mất phí thật.

---

## 20. Phụ lục mở rộng cho người triển khai

Phần phụ lục này tổng hợp thêm nhiều ghi chú thực chiến hơn cho người cài đặt, bảo trì, demo, chấm đồ án và mở rộng hệ thống.

Mục tiêu của phần này là:
* Giúp người mới có thể hiểu tổng thể dự án mà không cần đọc hết source code ngay từ đầu.
* Giúp người hướng dẫn hoặc người chấm có thể nắm nhanh kiến trúc và các quyết định triển khai.
* Giúp nhóm phát triển có thêm tài liệu vận hành nội bộ bằng tiếng Việt.
* Giúp việc chuyển giao dự án cho thành viên khác trong nhóm diễn ra nhẹ nhàng hơn.

---

## 21. Bức tranh tổng quan hệ thống

Hãy hình dung hệ thống dưới dạng chuỗi xử lý:

1. Admin tạo người dùng và môn học.
2. Admin phân công giảng viên phụ trách môn học.
3. Lecturer đăng nhập, chọn môn học mình được phân công.
4. Lecturer tải tài liệu PDF hoặc DOCX.
5. Hệ thống đọc tài liệu, tách nội dung, chia nhỏ thành nhiều đoạn.
6. Hệ thống gọi API embedding để tạo vector cho từng đoạn.
7. Các vector được lưu vào PostgreSQL với extension pgvector.
8. Student đăng nhập, mở chat trong một môn học cụ thể.
9. Câu hỏi của Student được chuyển thành embedding.
10. Hệ thống tìm các đoạn tài liệu gần nhất về mặt ngữ nghĩa.
11. Các đoạn phù hợp được đưa vào prompt gửi cho OpenRouter.
12. Mô hình AI sinh ra câu trả lời.
13. Câu trả lời được lưu lại cùng với metadata nguồn tham chiếu.
14. Nếu Student dùng gói Basic hoặc Premium, hệ thống sẽ trừ hạn mức theo cấu hình hiện hành.
15. Khi Student nâng cấp gói, PayOS sẽ tạo payment link.
16. Sau thanh toán, callback hoặc webhook sẽ đồng bộ trạng thái giao dịch.
17. Tài khoản được nâng cấp Premium và được tăng hạn mức.

Điểm quan trọng ở đây là:
* Chat không trả lời từ “kiến thức trống”.
* Chat cố gắng trả lời dựa trên tài liệu nội bộ.
* Dữ liệu vector là phần cốt lõi để tạo trải nghiệm RAG.
* Subscription chỉ là lớp business bao phía trên để kiểm soát quyền lợi người dùng.

---

## 22. Tư duy thiết kế chính

Hệ thống ưu tiên:
* Dễ hiểu cho đồ án.
* Dễ chạy trên máy cá nhân.
* Không yêu cầu hạ tầng quá phức tạp.
* Dễ demo với luồng người dùng rõ ràng.
* Giữ business logic tương đối tập trung trong Business layer.

Hệ thống chưa theo hướng:
* Microservices.
* Event-driven phức tạp.
* CQRS đầy đủ.
* Message broker như RabbitMQ hoặc Kafka.
* Scale ngang nhiều node.

Điều này là hợp lý cho bài toán đồ án hoặc MVP học thuật.

---

## 23. Ý nghĩa từng layer trong solution

### 23.1 Presentation Layer

`EduChatbot.Web` chịu trách nhiệm:
* Nhận request HTTP.
* Render Razor Pages.
* Nhận form submit.
* Trả JSON ở các endpoint nhỏ nếu cần.
* Chứa asset frontend như CSS, JS.
* Khởi tạo app ở `Program.cs`.

Đây là nơi phù hợp để xử lý:
* Routing.
* UI state.
* TempData.
* ViewModel để render dữ liệu.
* Các redirect sau thao tác người dùng.

Không nên nhồi business logic nặng vào đây.

### 23.2 Business Layer

`EduChatbot.Business` là trái tim của hệ thống.

Nó xử lý:
* Chat flow.
* Embedding flow.
* Payment flow.
* Subscription flow.
* Email queue flow.
* Rule upload document.
* Các quyết định nghiệp vụ như hạn mức, trạng thái, quyền lợi.

Khi cần thay đổi hành vi hệ thống, đa phần bạn sẽ sửa ở layer này trước.

### 23.3 Data Layer

`EduChatbot.Data` chịu trách nhiệm:
* Định nghĩa `ApplicationDbContext`.
* Mapping entity.
* Repository truy vấn dữ liệu.
* EF Core migrations.
* Seed dữ liệu danh tính.

Nếu có lỗi kiểu:
* Thiếu cột.
* Thiếu bảng.
* Query mapping sai.
* Foreign key sai.

thì thường nguyên nhân nằm ở Data layer.

### 23.4 Models Layer

`EduChatbot.Models` chứa:
* Entity.
* Enum.
* Identity model.
* DTO/result model dùng chung.

Lợi ích:
* Tránh vòng phụ thuộc không cần thiết.
* Dễ chia sẻ model cho nhiều layer.

---

## 24. Vai trò người dùng trong hệ thống

Hệ thống hiện có 3 role chính:
* Admin
* Lecturer
* Student

### 24.1 Admin

Admin có quyền:
* Tạo tài khoản.
* Quản lý role.
* Quản lý giảng viên.
* Quản lý sinh viên.
* Quản lý môn học.
* Phân công môn học cho giảng viên.
* Theo dõi dashboard tổng quan.

Admin không phải role dùng trực tiếp tính năng học tập hằng ngày.

### 24.2 Lecturer

Lecturer là người cung cấp tri thức cho hệ thống.

Lecturer có quyền:
* Đăng nhập.
* Xem môn học được phân công.
* Upload tài liệu.
* Quản lý tài liệu liên quan.

Lecturer không nên upload tài liệu cho môn không được phân công.

### 24.3 Student

Student là người dùng chính của chatbot.

Student có quyền:
* Đăng nhập.
* Chọn môn học.
* Tạo conversation.
* Gửi câu hỏi cho chatbot.
* Xem hạn mức.
* Xem gói dịch vụ.
* Nâng cấp Premium.

Subscription hiện chủ yếu nhắm vào Student.

---

## 25. Danh sách entity quan trọng

Các entity đáng chú ý:
* `ApplicationUser`
* `Document`
* `DocumentChunk`
* `Course`
* `LecturerCourse`
* `ChatConversation`
* `ChatMessage`
* `PaymentTransaction`
* `EmailQueue`

### 25.1 ApplicationUser

Đây là bảng user chính dựa trên ASP.NET Identity.

Ngoài các field chuẩn của Identity, dự án bổ sung:
* `FullName`
* `SubscriptionType`
* `AccountTitle`
* `TokenLimit`
* `UsedTokens`

Ý nghĩa:
* `SubscriptionType`: xác định Basic/Premium.
* `AccountTitle`: tên hiển thị gói.
* `TokenLimit`: hạn mức được phép dùng.
* `UsedTokens`: mức đã dùng.

### 25.2 Document

Đại diện cho file gốc được tải lên.

Chứa các thông tin như:
* Tên file.
* Đường dẫn file.
* Người upload.
* Môn học liên quan.
* Trạng thái.
* Metadata phục vụ review.

### 25.3 DocumentChunk

Đây là bảng rất quan trọng cho RAG.

Mỗi `DocumentChunk` là:
* Một đoạn nội dung nhỏ.
* Có index vị trí trong file.
* Có vector embedding.

Khi user chat, hệ thống chủ yếu query vào bảng này.

### 25.4 Course

Lưu:
* Mã môn học.
* Tên môn học.
* Mô tả.

Đây là trục phân vùng nghiệp vụ chính.

### 25.5 LecturerCourse

Đây là bảng phân công many-to-many giữa Lecturer và Course.

Business rule chính:
* Lecturer chỉ được upload cho môn có trong bảng này.

### 25.6 ChatConversation

Đại diện cho một cuộc hội thoại.

Gắn với:
* `UserId`
* `CourseId`
* tiêu đề cuộc trò chuyện

### 25.7 ChatMessage

Lưu:
* Role: user/ai
* Nội dung
* Nguồn tham chiếu nếu có

### 25.8 PaymentTransaction

Đây là entity trung tâm cho phần payment.

Lưu:
* `OrderCode`
* `Amount`
* `Provider`
* `Status`
* `CheckoutUrl`
* `PayOSPaymentLinkId`
* thời gian tạo và hoàn tất

### 25.9 EmailQueue

Bảng này dùng để tách hành vi gửi mail ra khỏi request tạo account.

Lợi ích:
* Tạo account nhanh hơn.
* Gửi mail retry được.
* Giảm lỗi do SMTP làm hỏng flow chính.

---

## 26. Luồng upload tài liệu chi tiết

Chi tiết hơn, một tài liệu đi qua các bước sau:

1. Lecturer mở form upload.
2. Lecturer chọn môn học.
3. Lecturer chọn file.
4. Server nhận file.
5. Hệ thống kiểm tra quyền Lecturer với môn học.
6. Hệ thống kiểm tra extension file.
7. Hệ thống lưu file vật lý vào `wwwroot/uploads/documents`.
8. Hệ thống extract text từ file.
9. Hệ thống chia text thành các chunk nhỏ.
10. Hệ thống gọi embedding service cho từng chunk.
11. Hệ thống lưu `Document`.
12. Hệ thống lưu nhiều `DocumentChunk`.
13. Tài liệu trở thành dữ liệu tra cứu cho chatbot.

Đây là nơi có thể phát sinh lỗi nhiều nhất khi demo vì phụ thuộc:
* file đầu vào
* quyền upload
* API embedding
* database vector

---

## 27. Luồng chat chi tiết

Khi Student gửi một câu hỏi:

1. Hệ thống xác thực người dùng.
2. Hệ thống kiểm tra conversation tồn tại và thuộc về user đó.
3. Hệ thống kiểm tra hạn mức hiện tại của user.
4. Hệ thống lưu câu hỏi của user vào DB.
5. Hệ thống tạo embedding cho câu hỏi.
6. Hệ thống search chunk gần nhất theo cosine similarity trong phạm vi môn học.
7. Hệ thống dựng context từ các chunk phù hợp.
8. Hệ thống gửi prompt sang OpenRouter.
9. Hệ thống nhận response AI.
10. Hệ thống lưu response vào DB.
11. Hệ thống cập nhật `UsedTokens`.
12. Hệ thống trả kết quả về UI.

Điểm mạnh:
* Có giới hạn phạm vi theo môn.
* Có source metadata.
* Có conversation history.

Điểm cần lưu ý:
* Nếu API AI chết thì user vẫn thấy message lỗi trả về.
* Nếu embedding API chết thì chat sẽ không chạy được.

---

## 28. Luồng subscription chi tiết

Subscription hiện dựa trên 2 tầng:
* tầng dữ liệu người dùng
* tầng transaction thanh toán

### 28.1 Basic

Basic là gói mặc định.

Đặc điểm:
* Miễn phí.
* Hạn mức thấp hơn.
* Quiz khóa.

### 28.2 Premium

Premium được kích hoạt khi:
* tạo payment link thành công
* thanh toán thành công
* callback hoặc webhook đồng bộ trạng thái thành công

Sau đó user được cập nhật:
* `SubscriptionType = PREMIUM`
* `AccountTitle = Premium`
* `TokenLimit = 100000`

### 28.3 Cách hệ thống quyết định gói hiện tại

Trang `/Subscription/Plans` và `/Subscription/Me` không dựa trên bảng gói riêng.

Thay vào đó:
* đọc `ApplicationUser.SubscriptionType`
* suy ra plan hiện hành

Đây là thiết kế đơn giản, phù hợp với số lượng gói ít.

---

## 29. Luồng thanh toán PayOS chi tiết

Luồng lý tưởng:

1. Student bấm `Nâng cấp Premium`.
2. Server kiểm tra user chưa phải Premium.
3. Server kiểm tra không có giao dịch pending trùng.
4. Server tạo `orderCode`.
5. Server gọi PayOS tạo payment link.
6. Server lưu `PaymentTransaction` ở trạng thái `PENDING`.
7. Server redirect user sang PayOS checkout.
8. User thanh toán hoặc hủy.
9. PayOS redirect về callback.
10. Hệ thống query lại trạng thái thật từ PayOS.
11. Nếu thành công, hệ thống cập nhật transaction `SUCCESS`.
12. Hệ thống nâng cấp user lên Premium.
13. Song song, webhook có thể gọi vào endpoint công khai để đồng bộ chắc chắn hơn.

Điểm đáng nhớ:
* Callback giúp UX cho user.
* Webhook giúp backend chắc chắn đồng bộ trạng thái.

---

## 30. Vì sao cần cả callback và webhook

Callback chỉ xảy ra khi:
* người dùng quay trở lại app sau thanh toán

Nhưng thực tế có thể xảy ra:
* user đóng tab
* user mất mạng
* redirect bị lỗi
* browser chặn

Khi đó webhook mới là nguồn xác nhận server-to-server đáng tin hơn.

Vì vậy:
* Callback tốt cho giao diện
* Webhook tốt cho dữ liệu

Với demo local:
* callback là đủ để trình diễn

Với demo công khai hoặc staging:
* nên bật webhook

---

## 31. Hướng dẫn cấu hình webhook rõ hơn

Để webhook hoạt động:

1. App phải chạy được.
2. DB phải chạy được.
3. App phải reachable từ internet.
4. `WebhookUrl` phải là URL public.
5. URL này phải trỏ đúng tới:
   * `/api/payment/payos/webhook`
6. `AutoConfirmWebhook` có thể bật để app tự đăng ký lúc startup.

Ví dụ:

```json
"PayOS": {
  "ClientId": "YOUR_CLIENT_ID",
  "ApiKey": "YOUR_API_KEY",
  "ChecksumKey": "YOUR_CHECKSUM_KEY",
  "WebhookUrl": "https://abc.ngrok-free.app/api/payment/payos/webhook",
  "AutoConfirmWebhook": true
}
```

Nếu confirm thất bại, hãy kiểm tra:
* app có chạy chưa
* ngrok có đúng port không
* URL public có đổi không
* endpoint có bị middleware khác chặn không

---

## 32. Tại sao localhost không dùng trực tiếp cho PayOS webhook

`localhost` chỉ có nghĩa trên chính máy của bạn.

Khi PayOS ở ngoài internet muốn gọi:
* nó không thể tự hiểu `localhost` là máy bạn

Do đó:
* callback URL public được
* webhook URL public bắt buộc

`ngrok` hoặc dịch vụ tunnel khác giúp biến:
* `http://localhost:5287`

thành:
* `https://abc.ngrok-free.app`

---

## 33. Cách dùng ngrok ổn định hơn cho demo

Đề xuất trình tự:

1. Start Docker DB.
2. Chạy app local.
3. Mở ngrok cho cổng app.
4. Copy URL public.
5. Cập nhật `WebhookUrl`.
6. Restart app nếu cần.
7. Mới bắt đầu test PayOS.

Không nên:
* mở ngrok trước khi app chạy
* bật AutoConfirmWebhook khi DB còn chết
* quên update URL khi ngrok đổi domain

---

## 34. Checklist trước khi demo thanh toán

Trước buổi demo, hãy kiểm tra:

* Docker đang chạy.
* PostgreSQL container đang `Up`.
* `dotnet run` không lỗi.
* Có thể login được.
* Có tài khoản Student để test.
* `OpenRouter:ApiKey` hợp lệ nếu cần demo chat.
* `PayOS` keys hợp lệ nếu cần demo thanh toán thật.
* `WebhookUrl` đang là URL public mới nhất.
* `AutoConfirmWebhook` đặt đúng theo mode demo.
* Ngrok không hết phiên.
* User đang ở gói Basic trước khi test nâng cấp.

Checklist ngắn:

* DB up
* App up
* ngrok up
* secrets đúng
* Student account sẵn sàng

---

## 35. Checklist sau khi demo thanh toán

Sau khi demo xong:

* Kiểm tra user đã thành Premium chưa.
* Kiểm tra `payment_transactions` có record mới chưa.
* Kiểm tra `Status` là `SUCCESS` hay không.
* Kiểm tra `TokenLimit` của user có tăng lên chưa.
* Kiểm tra trang `/Subscription/Me` đã phản ánh đúng chưa.

Nếu có bug:
* chụp lại log terminal
* chụp `orderCode`
* chụp màn hình callback
* query DB ngay lúc lỗi

---

## 36. Một số câu lệnh SQL hữu ích

Xem tất cả giao dịch:

```sql
SELECT *
FROM payment_transactions
ORDER BY created_at DESC;
```

Xem user nào đang Premium:

```sql
SELECT "Id", "Email", subscription_type, account_title, token_limit, used_tokens
FROM "AspNetUsers"
ORDER BY "Email";
```

Reset user về Basic:

```sql
UPDATE "AspNetUsers"
SET subscription_type = 'BASIC',
    account_title = 'Basic',
    token_limit = 5000,
    used_tokens = 0
WHERE "Email" = 'student@educhatbot.local';
```

Xóa giao dịch pending:

```sql
DELETE FROM payment_transactions
WHERE status = 'PENDING';
```

Đánh dấu giao dịch thành công để demo nội bộ:

```sql
UPDATE payment_transactions
SET status = 'SUCCESS',
    paid_at = NOW(),
    updated_at = NOW()
WHERE order_code = 123456789;
```

---

## 37. Khi nào nên reset dữ liệu local

Bạn nên reset dữ liệu local khi:
* migration cũ bị lệch nhiều
* dữ liệu test bị bẩn
* payment transactions tích tụ gây khó demo
* thay đổi lớn về schema vector

Cách nhẹ:
* xóa riêng dữ liệu test bằng SQL

Cách nặng:
* `docker-compose down -v`
* chạy lại migration

Chỉ reset nặng khi thật sự cần.

---

## 38. Vì sao nên tránh commit secret

Secret bị commit có thể gây:
* mất tiền nếu key thanh toán bị lộ
* bị spam email nếu SMTP bị lộ
* bị lạm dụng API AI
* ảnh hưởng cả team nếu repo public hoặc bị rò rỉ

Nguyên tắc:
* repo chỉ chứa giá trị mẫu
* giá trị thật nằm ở user-secrets hoặc môi trường deploy

---

## 39. Gợi ý cấu trúc cấu hình an toàn hơn

Nên tách:

* `appsettings.json`
  * chứa giá trị mặc định an toàn
* `appsettings.Development.json`
  * chứa config local không nhạy cảm
* `user-secrets`
  * chứa secret local
* biến môi trường
  * chứa secret ở staging/production

Các key nên đưa ra secret store:
* `OpenRouter:ApiKey`
* `Embedding:ApiKey`
* `Smtp:Username`
* `Smtp:Password`
* `PayOS:ClientId`
* `PayOS:ApiKey`
* `PayOS:ChecksumKey`

---

## 40. Cách trình bày dự án khi bảo vệ

Nếu cần thuyết trình, bạn có thể đi theo flow sau:

1. Giới thiệu bài toán.
2. Giới thiệu vai trò Admin, Lecturer, Student.
3. Trình bày kiến trúc 3 lớp.
4. Trình bày pipeline upload tài liệu.
5. Trình bày cách RAG search chunk theo embedding.
6. Trình bày subscription và hạn mức.
7. Trình bày PayOS payment flow.
8. Demo end-to-end.

Nên tập trung nhấn mạnh:
* vì sao dùng pgvector
* vì sao dùng chunking
* vì sao có payment webhook
* vì sao có limit usage

---

## 41. Các câu hỏi phản biện thường gặp

### 41.1 Tại sao dùng Razor Pages thay vì React/Vue?

Trả lời gợi ý:
* Razor Pages phù hợp đồ án backend-centric.
* Dễ triển khai nhanh.
* Đủ để chứng minh logic nghiệp vụ.

### 41.2 Tại sao dùng PostgreSQL + pgvector?

* Vì cần lưu embedding.
* Vì pgvector phổ biến cho bài toán RAG.
* Vì tận dụng luôn một DB thay vì thêm vector DB riêng.

### 41.3 Tại sao không dùng token thật mà đếm lượt hỏi?

* Dễ giải thích cho demo.
* Dễ theo dõi trực quan.
* Phù hợp quy mô đồ án.

### 41.4 Tại sao cần webhook nếu đã có callback?

* Callback phục vụ user flow.
* Webhook phục vụ đồng bộ server-to-server đáng tin cậy hơn.

### 41.5 Nếu AI trả lời sai thì sao?

* Hệ thống không đảm bảo tuyệt đối.
* Đã giới hạn context theo tài liệu nội bộ.
* Có nguồn tham chiếu để user tự kiểm chứng.

---

## 42. Những hạn chế hiện tại

Một tài liệu tốt nên thành thật về giới hạn hiện có.

Hệ thống hiện còn các giới hạn như:
* Chat context có thể vượt giới hạn nếu tài liệu quá dài.
* Quyền lợi `Quiz` có thể chưa được enforce ở mọi feature nếu module quiz chưa hoàn chỉnh.
* Hạn mức hiện dựa trên usage đơn giản, chưa có scheduler reset tinh vi.
* Payment hiện mới xoay quanh PayOS.
* Chưa có dashboard vận hành riêng cho payment.
* Chưa có test tự động đầy đủ cho callback/webhook.

Nêu giới hạn không làm dự án yếu đi.
Ngược lại, nó cho thấy nhóm hiểu rõ phạm vi và roadmap.

---

## 43. Đề xuất mở rộng sau này

Nếu có thời gian phát triển thêm, có thể làm:
* quiz generator thật sự dùng tài liệu môn học
* lecturer review answer quality
* analytics về câu hỏi phổ biến
* dashboard payment
* lịch sử nâng cấp gói
* giới hạn theo tháng thay vì reset đơn giản
* role-based audit log
* soft delete tài liệu
* background job chuẩn hơn cho xử lý tài liệu nặng
* cache cho conversation hot

---

## 44. Chuẩn đặt tên trong hệ thống

Một số pattern đang thấy:
* Bảng PostgreSQL dùng snake_case.
* C# property dùng PascalCase.
* Enum viết hoa theo business meaning.
* Razor PageModel đặt hậu tố `Model`.

Lợi ích:
* DB dễ đọc ở DBeaver.
* C# vẫn giữ phong cách .NET.

---

## 45. Quy ước commit gợi ý cho nhóm

Để repo dễ đọc hơn, có thể dùng:

* `feat: add payos webhook confirmation`
* `fix: prevent duplicate pending premium payment`
* `docs: expand Vietnamese setup guide`
* `refactor: simplify chat token usage flow`
* `chore: update docker compose defaults`

Không bắt buộc, nhưng rất có ích khi review lịch sử thay đổi.

---

## 46. Hướng dẫn đọc source code cho người mới vào nhóm

Thứ tự đọc gợi ý:

1. `README.md`
2. `README_VI.md`
3. `EduChatbot.Web/Program.cs`
4. `EduChatbot.Business/DependencyInjection.cs`
5. `EduChatbot.Data/ApplicationDbContext.cs`
6. `EduChatbot.Models/Identity/ApplicationUser.cs`
7. `ChatService`
8. `DocumentService`
9. `PayOSPaymentService`
10. `Subscription` pages

Nếu đọc theo thứ tự này, bạn sẽ hình dung tổng thể nhanh hơn rất nhiều.

---

## 47. Hướng dẫn đọc phần Payment

Thứ tự nên đọc:

1. `PaymentTransaction` entity
2. `PaymentStatus` enum
3. `PaymentProvider` enum
4. `IPaymentTransactionRepository`
5. `PaymentTransactionRepository`
6. `IPayOSPaymentService`
7. `PayOSPaymentService`
8. `Plans.cshtml.cs`
9. `Callback.cshtml.cs`
10. `Program.cs` phần webhook

Sau khi đọc xong các file này, bạn gần như hiểu hết flow thanh toán.

---

## 48. Hướng dẫn đọc phần Subscription

Nên đọc:

1. `SubscriptionType`
2. `ApplicationUser`
3. `SubscriptionPlanViewModel`
4. `MySubscriptionViewModel`
5. `ISubscriptionService`
6. `SubscriptionService`
7. `Plans.cshtml`
8. `Me.cshtml`

Điểm quan trọng:
* Subscription hiện là business logic nhẹ.
* Chưa tách thành engine gói phức tạp.

---

## 49. Hướng dẫn đọc phần Chat

Nên đọc:

1. `IChatRepository`
2. `ChatRepository`
3. `IEmbeddingService`
4. `OpenRouterEmbeddingService`
5. `IChatService`
6. `ChatService`
7. các Razor Pages chat

Khi đọc `ChatService`, tập trung vào:
* lưu user message
* search chunk
* build context
* call AI
* lưu AI message
* cập nhật usage

---

## 50. Gợi ý test manual tối thiểu

Một vòng test tay tối thiểu nên bao gồm:

### 50.1 Identity

* Login admin thành công
* Tạo student thành công
* Tạo lecturer thành công

### 50.2 Course

* Tạo course thành công
* Phân công lecturer thành công

### 50.3 Upload

* Lecturer upload file đúng định dạng thành công
* Lecturer upload sai môn bị chặn

### 50.4 Chat

* Student tạo conversation
* Student gửi câu hỏi
* AI trả lời có dữ liệu
* `UsedTokens` tăng

### 50.5 Subscription

* Student thấy Basic
* Student bấm upgrade
* Callback thành công
* Student thành Premium

---

## 51. Kịch bản demo 5 phút

Nếu thời gian demo ngắn, bạn có thể làm:

1. Login admin
2. Cho xem dashboard
3. Login lecturer
4. Upload 1 tài liệu
5. Login student
6. Chat 1 câu hỏi
7. Mở trang subscription
8. Bấm nâng cấp Premium
9. Quay về callback
10. Mở `/Subscription/Me`

Điểm mạnh của flow này:
* có đầy đủ 3 role
* có AI
* có payment
* có data flow end-to-end

---

## 52. Kịch bản demo 10 phút

Nếu có thêm thời gian:

1. Giải thích architecture 1 phút
2. Tạo course 1 phút
3. Tạo lecturer và student 1 phút
4. Upload tài liệu 2 phút
5. Chat 2 phút
6. Nâng cấp Premium 2 phút
7. Xem DB hoặc trang subscription 1 phút

---

## 53. Kịch bản demo khi internet chập chờn

Nếu mạng không ổn:

* Không phụ thuộc chat nhiều.
* Chuẩn bị sẵn DB có dữ liệu.
* Nếu cần, ưu tiên demo upload, quản lý role, payment mock hoặc callback đã chuẩn bị.
* Mở sẵn trang `/Subscription/Plans` và `/Subscription/Me`.

Nếu API AI hỏng:
* vẫn có thể demo architecture, upload, auth, payment, subscription.

---

## 54. Tại sao README tiếng Việt quan trọng

README tiếng Việt giúp:
* nhóm làm đồ án cùng nhau dễ hiểu hơn
* người chấm trong nước đọc nhanh hơn
* hỗ trợ onboarding thành viên mới
* giảm lệ thuộc vào giải thích miệng

Tài liệu càng rõ, thời gian support càng ít.

---

## 55. Gợi ý bổ sung ảnh minh họa sau này

Bạn có thể thêm các ảnh sau để README hấp dẫn hơn:

* sơ đồ flow upload tài liệu
* sơ đồ flow chat + RAG
* sơ đồ flow PayOS callback/webhook
* ảnh dashboard admin
* ảnh trang chat student
* ảnh trang subscription plans
* ảnh callback thành công

Ảnh không bắt buộc, nhưng rất tốt cho tài liệu bàn giao.

---

## 56. Ví dụ flow lỗi Payment và cách hiểu

### Case 1: User bấm upgrade nhiều lần

Hệ thống sẽ:
* chặn do đang có `PENDING`
* tránh tạo trùng payment link

### Case 2: User thanh toán nhưng đóng tab

Hệ thống vẫn có thể đồng bộ qua webhook nếu webhook hoạt động.

### Case 3: User quay về callback nhưng PayOS chưa kịp phản ánh

Hệ thống có thể tạm báo đang chờ xử lý.

### Case 4: Key PayOS sai

Tạo payment link sẽ fail ngay.

---

## 57. Ví dụ flow lỗi Chat và cách hiểu

### Case 1: Không có tài liệu nào

AI có thể trả lời từ general knowledge hoặc báo thiếu nguồn, tùy xử lý hiện hành.

### Case 2: Embedding API lỗi

Chat search context không chạy được.

### Case 3: OpenRouter lỗi 401

Key sai hoặc hết hiệu lực.

### Case 4: User vượt hạn mức

Hệ thống nên chặn gửi chat mới.

---

## 58. Ví dụ flow lỗi Upload và cách hiểu

### Case 1: File hỏng

Extract text có thể fail.

### Case 2: File quá lớn

Có thể gây chậm hoặc lỗi bộ nhớ.

### Case 3: Lecturer không được phân công

Bị chặn ở business rule.

### Case 4: Embedding call thất bại giữa chừng

Document có thể lưu dang dở nếu flow chưa có transaction đầy đủ.

---

## 59. Câu hỏi thường gặp nội bộ nhóm

### Hỏi: Có thể bỏ Docker không?

Trả lời:
* Có thể nếu tự cài PostgreSQL + pgvector.
* Nhưng Docker vẫn nhanh hơn cho phần demo.

### Hỏi: Có thể đổi AI provider không?

* Có, nếu thay service tương ứng.

### Hỏi: Có thể đổi PayOS sang MoMo không?

* Có, nhưng cần viết lại payment integration.

### Hỏi: Có thể deploy cloud không?

* Có, miễn cấu hình đúng DB, secret và webhook URL.

---

## 60. Mẫu checklist deploy thử nghiệm

Checklist staging:

* Build release thành công
* DB production/staging tạo schema xong
* Secret đã inject
* Connection string đúng
* OpenRouter key đúng
* PayOS key đúng
* Domain public chạy HTTPS
* Webhook URL public đúng
* Seed admin thành công
* Tài khoản test có sẵn

---

## 61. Mẫu checklist bàn giao project

Khi bàn giao cho người khác, nên bàn giao:

* source code
* file README chính
* README tiếng Việt
* sơ đồ kiến trúc
* danh sách secret cần cấu hình
* tài khoản demo
* lệnh run
* lệnh DB
* lệnh ngrok
* flow test subscription/payment

---

## 62. Glossary thuật ngữ

### RAG

Retrieval-Augmented Generation.

Nghĩa là:
* tìm dữ liệu liên quan trước
* rồi mới đưa vào LLM để trả lời

### Embedding

Biểu diễn vector của text.

Giúp:
* so sánh độ gần nghĩa
* search ngữ nghĩa

### Chunk

Đoạn nhỏ tách ra từ tài liệu lớn.

### Cosine Similarity

Độ đo mức gần nhau giữa 2 vector.

### Webhook

HTTP callback từ server bên ngoài gọi về app của bạn.

### Callback

Redirect hoặc response quay lại flow user sau một hành động.

### pgvector

Extension PostgreSQL cho phép lưu và truy vấn vector.

### EF Core Migration

Cơ chế version schema DB bằng code.

### Identity

Framework quản lý user, role, password, auth của ASP.NET Core.

---

## 63. Kết luận tài liệu

Nếu bạn đọc đến đây, bạn đã có:
* cái nhìn kiến trúc tổng thể
* hướng dẫn cài đặt
* hướng dẫn demo
* hướng dẫn payment
* hướng dẫn troubleshoot
* hướng dẫn bàn giao

Mục tiêu của `README_VI.md` là giúp bất kỳ thành viên nào cũng có thể:
* chạy được project
* hiểu được project
* demo được project
* sửa được project ở mức cơ bản

Nếu dự án tiếp tục phát triển, hãy duy trì thói quen:
* cập nhật README sau mỗi thay đổi lớn
* cập nhật flow demo nếu nghiệp vụ đổi
* cập nhật phần secret/config khi thêm dịch vụ mới

Tài liệu tốt không làm chậm phát triển.
Tài liệu tốt làm cho tốc độ của cả nhóm bền vững hơn.
