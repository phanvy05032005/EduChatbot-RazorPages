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

### 12.5 Xử lý giao dịch chờ giả lập (Pending Payment Bypass)
* Khi bạn bấm nâng cấp Premium nhưng không thanh toán mà quay lại ứng dụng, hệ thống sẽ báo: *"Bạn đang có một giao dịch Premium chờ xử lý"*.
* Hệ thống khóa việc tạo giao dịch mới trong vòng 15 phút để chờ giao dịch cũ hết hạn (stale pending).
* Để reset nhanh phục vụ demo mà không cần đợi 15 phút, bạn có thể thực hiện chạy câu lệnh SQL này trong database:
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
