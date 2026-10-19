# تقرير فحص مشروع التجارة الإلكترونية

تاريخ الفحص: 19 أكتوبر 2026

## الخلاصة التنفيذية

المشروع يملك أساسًا جيدًا لتجربة معمارية Microservices على .NET 10، مع تقسيم Domain/Application/Infrastructure/API، واستخدام gRPC وRabbitMQ وWolverine وEF Core، إضافة إلى اختبارات وحدات معتبرة في Auth وOrder وProduct. لكنه غير جاهز حاليًا للتشغيل الموثوق أو الإنتاج؛ توجد ثغرة أسرار حرجة، وبنية تشغيل محلية ناقصة، وإعدادات تعتمد على localhost، ولا يوجد مسار CI/CD أو تحقق كامل من البناء والاختبارات.

التقدير الحالي: **Prototype متقدم، وليس Production-ready**.

## أهم النواقص حسب الأولوية

### P0 — يجب إصلاحها فورًا

1. **سر SMTP موجود محليًا في ملف `.env`**
   - الدليل: `E-Commerce_Email_Service/.env:1-5` يحتوي بيانات SMTP فعلية.
   - الملف مستبعد من Git بواسطة `.gitignore`، لكنه موجود في نسخة العمل وقد يكون تسرب سابقًا عبر مشاركة أو نسخة احتياطية.
   - الإجراء: تدوير كلمة مرور التطبيق فورًا، التأكد من تاريخ Git والنسخ الاحتياطية، حذف السر من بيئة التطوير، واستخدام User Secrets/Secret Manager/متغيرات بيئة في التشغيل.

2. **البناء الكامل غير قابل للتحقق في الحالة الحالية**
   - أغلب المشاريع كانت تفتقد `obj/project.assets.json`.
   - بعد محاولة Restore، فشل البناء في مشاريع متعددة بسبب عدم الوصول إلى NuGet (`NU1301`)، لذلك لا يمكن اعتبار المشروع Build-passing.
   - الإجراء: إضافة CI ينفذ `restore`, `build`, `test` على كل الحلول، وتثبيت مصدر NuGet موثوق أو caching داخلي.

3. **لا توجد بيئة تشغيل كاملة**
   - `E-Commerce_GatewayDemo/docker-compose.yml` يحتوي على الخدمات الأساسية في قسم معلّق، والجزء الفعال يعرّف Gateway فقط.
   - لا توجد حاويات SQL Server أو RabbitMQ أو Auth/Product/Cart/Order/Payment/Email متصلة معًا.
   - الإجراء: إنشاء Compose موحد للتطوير، مع networks وvolumes وhealthchecks وملفات environment منفصلة.

### P1 — تمنع الاعتمادية والإطلاق الآمن

4. **عناوين خدمات ثابتة على localhost**
   - Query Gateway يستخدم `https://localhost:7103/graphql` و`https://localhost:7046/graphql` في `E-Commerce-QueryGatewayDemo/QueryGateqay/Program.cs:66-71`.
   - هذا يفشل داخل Docker أو Kubernetes، ويجعل تغيير البيئة يدويًا.
   - الإجراء: نقل العناوين إلى Options/Environment variables واستخدام أسماء الخدمات داخل الشبكة.

5. **CORS مفتوح بالكامل على Gateway**
   - `E-Commerce_GatewayDemo/E-Commerce-Gateway/Program.cs:92-94` يستخدم `AllowAnyOrigin`, `AllowAnyMethod`, `AllowAnyHeader`.
   - الإجراء: قائمة Origins معروفة لكل بيئة، مع فصل إعدادات Development عن Production.

6. **Health checks غير مكتملة**
   - Gateway فقط يملك فحصًا شكليًا يعيد Healthy؛ لا يتحقق من SQL Server أو RabbitMQ أو الخدمات الخلفية.
   - لا توجد جاهزية حقيقية تميّز بين liveness وreadiness.
   - الإجراء: إضافة health checks للاتصالات الفعلية، واستخدام readiness في orchestrator.

7. **غياب إعدادات Production وSecret Management**
   - معظم الخدمات تحتوي فقط على `appsettings.Development.json`، ولا يوجد معيار موحد لتكوين Production.
   - الإجراء: Options validation عند بدء التشغيل، أمثلة `.env.example` بدون أسرار، وSecret Manager خارجي.

8. **عدم اتساق إصدارات الحزم**
   - HotChocolate في Product يستخدم أكثر من إصدار (`15.1.14` و`15.1.15/15.1.16`).
   - Wolverine وMassTransit وEF Core تختلف إصداراتها بين الخدمات.
   - توجد حزمة `EntityFramework` قديمة جدًا (`6.2.0`) داخل Order Infrastructure بجانب EF Core 10.
   - الإجراء: Central Package Management أو Directory.Packages.props، وتوحيد الإصدارات وإزالة الحزم غير الضرورية.

9. **تحذيرات ثغرات في الاعتمادات**
   - Restore/Build لخدمة البريد أظهر `NU1902` لـ MailKit و`NU1902/NU1903` لـ System.Security.Cryptography.Pkcs، بما في ذلك تحذير High.
   - الإجراء: تحديث الحزم المتأثرة وتشغيل `dotnet list package --vulnerable --include-transitive` داخل CI.

### P2 — نقص في الجودة والتشغيل

10. **اختبارات غير متوازنة**
    - توجد 40 ملفات اختبار تقريبًا و179 `[Fact]` و13 `[Theory]`، متركزة في Auth/Order/Product.
    - لا تظهر تغطية مكافئة لـ Cart وPayment وEmail وGateway وQuery Gateway، ولا اختبارات Contract/Integration عبر RabbitMQ وgRPC وSQL Server.
    - الإجراء: إضافة Integration tests مع Testcontainers، وContract tests للـ proto/events، وقياس coverage بحد أدنى معلن.

11. **غياب Observability موحدة**
    - يوجد Serilog في بعض الخدمات، لكن لا يوجد معيار واضح للـ correlation/trace IDs أو OpenTelemetry أو metrics.
    - الإجراء: OpenTelemetry traces/metrics/logs، correlation ID، dashboards وتنبيهات.

12. **غياب سياسات موحدة للرسائل**
    - توجد محاولات Idempotency متعددة ومخصصة لكل خدمة، لكن لا يوجد توثيق/اختبار يثبت سلوك retry وdead-letter وordering وoutbox.
    - الإجراء: اعتماد Outbox/Inbox موحد، سياسات retry وDLQ، ومراقبة الرسائل الفاشلة.

13. **التوثيق التشغيلي غير مكتمل**
    - README يصف طموحًا معماريًا واسعًا، لكنه لا يقدم وصفة قابلة لإعادة الإنتاج لتشغيل كل الاعتماديات، أو migrations، أو بيانات اختبار، أو troubleshooting.
    - الإجراء: إضافة Quickstart فعلي، جدول منافذ، مخطط dependencies، أوامر migrations، أمثلة requests، وحسابات seed آمنة.

14. **تعدد الحلول وملفات Solution فارغة/مكررة**
    - توجد ملفات `.sln` و`.slnx` لنفس الخدمات، وبعض ملفات `.slnx` تحتوي `<Solution />` فقط.
    - توجد أيضًا ملفات تجريبية مثل `WolverineTest` و`WolverineTypeCheck.cs` خارج مسار المنتج.
    - الإجراء: اختيار solution strategy واحدة، إنشاء solution root أو مشروع orchestration واضح، ونقل/حذف التجارب من مسار البناء.

## ما هو موجود بشكل جيد

- تقسيم طبقات واضح في Auth/Product/Order/Cart.
- وجود migrations لكل قواعد بيانات الخدمات الأساسية.
- استخدام validation وmapping وDTOs بدل تمرير الكيانات مباشرة في معظم المسارات.
- وجود اختبارات Domain وApplication وRepository وgRPC لبعض الخدمات.
- وجود Idempotency components وgRPC interceptors وWolverine messaging كاتجاه تصميم مناسب.
- وجود Gateway authentication وrate limiting وexception handling وhealth endpoint مبدئي.

## خطة تنفيذ مقترحة

### المرحلة الأولى: تأمين وتشغيل محلي

1. تدوير SMTP secret وفحص Git history.
2. إضافة `env.example` وتوثيق المتغيرات المطلوبة.
3. بناء Compose كامل لـ SQL Server وRabbitMQ وجميع الخدمات.
4. نقل كل service URLs وJWT وconnection strings إلى configuration.
5. إصلاح CORS وHealth checks وإضافة migrations startup موثقة.

### المرحلة الثانية: تثبيت البناء والجودة

1. توحيد الحزم عبر `Directory.Packages.props`.
2. إضافة CI: restore/build/test/vulnerability scan.
3. إنشاء اختبارات Integration وContract للخدمات التي لا تملك تغطية.
4. إضافة coverage gate وformat/analyzer rules.

### المرحلة الثالثة: الجاهزية الإنتاجية

1. OpenTelemetry وcorrelation IDs وmetrics وdashboards.
2. Outbox/Inbox وDLQ وretry policies موحدة.
3. Kubernetes/Helm أو manifests مناسبة مع secrets وreadiness/liveness.
4. توثيق API وevents وproto versions وbackward compatibility.
5. Load testing وsecurity testing وdisaster recovery للبيانات.

## حدود الفحص

تم إجراء فحص ساكن للمشروع، مراجعة ملفات الحلول والمشاريع والإعدادات وDocker وREADME، ومحاولة Restore/Build. تعذر إثبات نجاح البناء والاختبارات بسبب عدم توفر الوصول إلى `api.nuget.org` من بيئة الفحص؛ لذلك يجب إعادة تشغيل CI بعد توفير مصدر NuGet.
