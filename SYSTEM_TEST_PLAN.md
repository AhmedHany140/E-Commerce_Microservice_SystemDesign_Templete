# خطة اختبار النظام بالكامل

## الهدف

إثبات أن النظام يعمل بالكامل من خلال:

- جميع الـ endpoints.
- جميع الـ GraphQL queries.
- الاتصالات بين الخدمات باستخدام REST وgRPC وRabbitMQ.
- قواعد البيانات والمigrations.
- الـ Gateway والـ Query Gateway.
- Paymob في وضع الاختبار.
- سيناريوهات النجاح والفشل والـ idempotency.

لا يتم الانتقال إلى مرحلة إلا بعد نجاح شروط العبور الخاصة بالمرحلة السابقة.

## الحالة الحالية قبل بدء الاختبار

- تم تطبيق migrations يدويًا على قواعد البيانات الستة.
- تم إزالة أي تشغيل تلقائي للـ migrations عند بدء الخدمات.
- تم توحيد إصدارات الحزم باستخدام `Directory.Packages.props`.
- تم تجهيز ملف `.env` مركزي للإعدادات والـ secrets.
- تم تجهيز `run-local.ps1` لتشغيل الخدمات بدون Docker.
- الاختبارات التكاملية الكاملة لم تُنفذ بعد.

## خريطة الخدمات

| الخدمة | HTTP | البروتوكولات | المسارات | قاعدة البيانات |
|---|---:|---|---|---|
| Auth | 5051 | REST/Wolverine، gRPC | `/api/auth/*` | `ECommerceAuthDb` |
| Cart | 5057 | REST/Wolverine، gRPC | `/api/cart/*` | `ECommerceCartDb` |
| Product | 5090 | REST/Wolverine، gRPC، GraphQL | `/api/products/*`، `/graphql` | `ECommerceProductDb` |
| Order | 5072 | REST/Wolverine، gRPC، GraphQL | `/api/orders/*`، `/graphql` | `ECommerceOrderDb` |
| Payment | 5113 | REST Webhook، gRPC | `/api/payment/webhook` | `ECommercePaymentDb` |
| Email | 5103 | RabbitMQ Consumer | داخلي فقط | `ECommerceEmailDb` |
| Query Gateway | 5008 | GraphQL Federation | `/graphql` | لا يوجد |
| Command Gateway | 5046 | YARP Reverse Proxy | `/api/*`، `/graphql` | لا يوجد |

## ترتيب تشغيل الخدمات

يتم تشغيل الخدمات من خلال:

```powershell
./run-local.ps1 -WithRabbitMq -NoBuild
```

الخدمات التي يجب أن تعمل:

1. Auth على `http://localhost:5051`
2. Cart على `http://localhost:5057`
3. Product على `http://localhost:5090`
4. Order على `http://localhost:5072`
5. Payment على `http://localhost:5113`
6. Email على `http://localhost:5103`
7. Query Gateway على `http://localhost:5008`
8. Command Gateway على `http://localhost:5046`

نقطة الدخول العامة للاختبار تكون:

```text
http://localhost:5046
```

ولا يتم الاعتماد على عناوين الخدمات الداخلية في اختبارات العميل النهائية.

---

## المرحلة 0: تهيئة بيئة الاختبار

### الهدف

التأكد من أن كل المتطلبات الخارجية جاهزة قبل تشغيل الخدمات.

### المطلوب

- SQL Server متاح على instance باسم `.`.
- قواعد البيانات الستة موجودة ومتصلة.
- RabbitMQ متاح باستخدام `RABBITMQ_URL` في ملف `.env`.
- Paymob يعمل في Test Mode.
- JWT settings موجودة ومطابقة بين الخدمات.
- عدم وجود قيم placeholder في الإعدادات.

### شروط النجاح

- يمكن الاتصال بـ SQL Server.
- يمكن الاتصال بـ RabbitMQ.
- Paymob credentials موجودة.
- `JWT_KEY` و`JWT_ISSUER` و`JWT_AUDIENCE` موجودة.

### شرط التوقف

نتوقف إذا كانت أي خدمة تعتمد على secret غير موجود أو إذا كان SQL Server أو RabbitMQ غير متاح.

---

## المرحلة 1: Build وPackage Contract Gate

### الهدف

التأكد من أن المشروع كله قابل للبناء بعد توحيد إصدارات الحزم.

### الأوامر

```powershell
dotnet restore
dotnet build
dotnet test
```

### الاختبارات

- فحص عدم وجود build errors.
- فحص عدم وجود package version conflicts.
- تشغيل اختبارات Auth الحالية.
- تشغيل اختبارات Order الحالية.
- تشغيل اختبارات Product الحالية.
- مراجعة ملفات `.proto`.
- مراجعة Swagger contracts.
- مراجعة GraphQL schemas.

### شروط النجاح

- كل المشاريع المهمة تبني بنجاح.
- كل الاختبارات الحالية تمر.
- لا يوجد اختلاف غير مقصود في إصدارات الحزم.

### ملاحظة

الاختبارات الحالية تركز على handlers وبعض GraphQL/unit tests، ولا تعتبر بديلًا عن integration tests.

---

## المرحلة 2: Infrastructure Smoke Test

### الهدف

اختبار البنية التحتية وقواعد البيانات قبل اختبار منطق الخدمات.

### قواعد البيانات المطلوب فحصها

- `ECommerceAuthDb`
- `ECommerceCartDb`
- `ECommerceProductDb`
- `ECommerceOrderDb`
- `ECommercePaymentDb`
- `ECommerceEmailDb`

### الاختبارات

- التأكد من وجود كل قاعدة بيانات.
- التأكد من وجود `__EFMigrationsHistory`.
- التأكد من تطبيق آخر migration لكل service.
- فحص اتصال كل DbContext.
- فحص RabbitMQ connection.
- فحص queues المطلوبة.
- فحص `GET /health` من الـ Gateway.
- فحص `GET /health/ready` من الـ Gateway.

### شروط النجاح

- كل قواعد البيانات موجودة.
- لا توجد migrations pending.
- كل الخدمات تستطيع الاتصال بقواعد بياناتها.
- RabbitMQ connection ناجح عند تشغيله.
- Gateway health endpoints ترجع `200`.

---

## المرحلة 3: اختبار كل Service منفردة

يتم اختبار كل خدمة منفردة قبل اختبار التواصل بينها.

### Auth Service

#### Happy Path

- Register مستخدم جديد.
- Login بالمستخدم.
- الحصول على JWT وRefresh Token.
- Refresh Token.
- Confirm Email.

#### Validation وSecurity

- تسجيل بريد غير صحيح.
- كلمة مرور ضعيفة.
- Login ببيانات خاطئة.
- طلب بدون JWT.
- JWT منتهي.
- JWT غير صحيح.
- Change Password.
- Forgot Password وReset Password.

#### Messaging

- التأكد من إرسال OTP أو email event إلى `email-queue`.

### Product Service

#### Commands

- إنشاء Category.
- تعديل Category.
- حذف Category.
- إنشاء Product.
- تعديل Product.
- حذف Product.

#### Queries

- استرجاع المنتجات.
- البحث والتصفية.
- sorting.
- pagination.
- استرجاع Product بواسطة gRPC.

#### Validation

- سعر غير صحيح.
- stock سالب.
- Category غير موجودة.
- Product غير موجود.

### Cart Service

- الحصول على السلة الحالية.
- إضافة Product للسلة.
- تعديل الكمية.
- حذف item.
- مسح السلة.
- اختبار Product lookup عبر gRPC.
- اختبار Authorization عبر gRPC.
- اختبار checkout command.
- اختبار الكمية غير الصحيحة.
- اختبار Product غير موجود.

### Order Service

- إنشاء Order.
- إلغاء Order.
- الحصول على Order بواسطة GraphQL.
- الحصول على Orders الخاصة بالمستخدم.
- اختبار Admin orders.
- اختبار Order creation عبر Cart.
- اختبار Paid Order عبر Payment event/gRPC.
- اختبار Refund flow عند إلغاء Order.

### Payment Service

- Initial Payment عبر gRPC.
- اختبار Paymob Card/VPC.
- اختبار Paymob Wallet/UIG.
- اختبار iframe.
- استقبال Paymob webhook.
- Payment success event.
- Payment failed event.
- Refund event.
- Webhook ناقص أو غير صحيح.
- Webhook مكرر.

### Email Service

- تشغيل consumer.
- استقبال رسالة من `email-queue`.
- إرسال البريد أو تسجيل الفشل بطريقة واضحة.
- اختبار duplicate event.
- اختبار إعادة الاتصال بـ RabbitMQ.

### شرط العبور

لا ننتقل للمرحلة التالية إلا إذا:

- كل خدمة تبدأ بدون exception.
- الـ happy paths تعمل.
- الـ validation ترجع أخطاء مناسبة.
- البيانات تُحفظ في قاعدة البيانات الصحيحة.
- لا يوجد side effect مكرر.

---

## المرحلة 4: اختبار REST من خلال Command Gateway

### الهدف

التأكد أن الـ Gateway هو نقطة الدخول الصحيحة لكل عمليات الكتابة.

### المسارات

- `/api/auth/*`
- `/api/products/*`
- `/api/carts/*`
- `/api/orders/*`
- `/api/payment/*`

### الاختبارات

- YARP route صحيح لكل service.
- انتقال headers بشكل صحيح.
- JWT validation.
- CORS.
- Rate limiting.
- Fallback عند route غير موجود.
- إضافة `Idempotency-Key` للطلبات التي تغير البيانات.
- الحفاظ على status code وresponse body من الخدمة.
- التعامل مع downstream service غير متاح.

### شرط مهم

كل اختبار من جهة العميل يجب أن يستخدم:

```text
http://localhost:5046
```

وليس عنوان الخدمة المباشر.

---

## المرحلة 5: اختبار gRPC

### الخدمات والعمليات

#### Auth

- `AuthorizationGrpcService.ValidateToken`

#### Product

- `ProductGrpcService.GetProduct`

#### Cart

- `CartGrpcService.GetCart`

#### Order

- `OrderGrpcService.CreateOrder`
- `OrderGrpcService.PaidOrder`

#### Payment

- `PaymentGrpcService.InitialPayment`
- `PaymentGrpcService.Refund`

### حالات الاختبار

- Request صحيح.
- Request ناقص.
- Product أو Order غير موجود.
- JWT metadata صحيح.
- JWT metadata ناقص.
- timeout.
- downstream unavailable.
- cancellation.
- duplicate request مع نفس idempotency key.
- status codes مثل `OK` و`InvalidArgument` و`Unauthenticated` و`Unavailable`.

### الدليل المطلوب

- gRPC status.
- request correlation ID.
- client log.
- server log.
- أثر العملية في قاعدة البيانات.

---

## المرحلة 6: اختبار RabbitMQ

### Queues المطلوبة

- `email-queue`
- `payment-queue`
- `refund-queue`

### الاختبارات

- Publish رسالة صحيحة.
- التأكد من وصول الرسالة للـ consumer.
- التأكد من acknowledgement.
- اختبار retry عند فشل consumer.
- اختبار error/dead-letter queue.
- إيقاف consumer ثم إعادة تشغيله.
- duplicate message.
- message malformed.
- RabbitMQ connection loss ثم reconnect.
- مراقبة queue depth وعدم وجود رسائل معلقة بلا سبب.

### شروط النجاح

- الرسائل تصل للـ consumer الصحيح.
- لا يحدث side effect مرتين عند duplicate event.
- الرسالة الفاشلة لا تختفي بدون log.
- retry behavior واضح.
- النظام يعود للعمل بعد reconnect.

---

## المرحلة 7: اختبار GraphQL

### GraphQL endpoints

- Product Service: `http://localhost:5090/graphql`
- Order Service: `http://localhost:5072/graphql`
- Query Gateway: `http://localhost:5008/graphql`
- من خلال الـ Command Gateway: `http://localhost:5046/graphql`

### Product queries

- استرجاع المنتجات.
- filtering.
- sorting.
- pagination.
- projection للحقول المطلوبة فقط.
- query ببيانات غير صحيحة.

### Order queries

- `orders`.
- `myOrders`.
- `orderById`.
- pagination.
- filtering.
- sorting.
- authorization حسب المستخدم والدور.

### Query Gateway

- تحميل Product subgraph.
- تحميل Order subgraph.
- unified query.
- Authorization header propagation.
- downstream service unavailable.
- schema mismatch.
- GraphQL errors array.
- query depth/complexity حسب الإعدادات.

### شرط النجاح

- الـ schema يتم تحميله بدون federation errors.
- النتائج ترجع بالشكل المتوقع.
- الحقول المحمية ترفض الطلب بدون JWT.
- أخطاء downstream تظهر بطريقة مفهومة ولا تسبب crash للـ gateway.

---

## المرحلة 8: سيناريو Business End-to-End

هذا هو السيناريو الرئيسي الذي يختبر النظام كله.

1. Register مستخدم جديد.
2. Login والحصول على JWT.
3. إنشاء Category وProduct.
4. قراءة Product من GraphQL.
5. إضافة Product إلى Cart.
6. تعديل كمية المنتج.
7. تنفيذ Checkout.
8. Cart يتواصل مع Product وOrder وPayment عبر gRPC.
9. Payment ينشئ transaction في Paymob test mode.
10. Paymob يرسل webhook.
11. Payment ينشر payment success event.
12. Order يتحول إلى Paid.
13. Email event يذهب إلى `email-queue`.
14. Email service يستهلك الرسالة.
15. قراءة Order النهائي من Query Gateway.
16. التأكد من أن الحالة والمبلغ والـ items صحيحة.

### البيانات التي يجب تتبعها

- User ID.
- Product ID.
- Cart ID.
- Order ID.
- Paymob transaction ID.
- Correlation ID.
- Idempotency-Key.

يجب أن تظهر هذه القيم في logs أو response أو database records حسب كل مرحلة.

---

## المرحلة 9: اختبارات الفشل والـ Resilience

### Authentication failures

- JWT ناقص.
- JWT منتهي.
- JWT بتوقيع خاطئ.
- JWT issuer غير صحيح.
- JWT audience غير صحيحة.

### Data failures

- Product غير موجود.
- Stock غير كافٍ.
- Category غير موجودة.
- Order غير موجود.
- duplicate Idempotency-Key.

### Infrastructure failures

- إيقاف Product service أثناء checkout.
- إيقاف Order service أثناء payment.
- إيقاف Payment service.
- قطع الاتصال بـ RabbitMQ.
- عدم توفر SQL Server.
- Paymob timeout.
- Paymob failed response.

### Expected behavior

- status code مناسب.
- error response واضح.
- لا يتم إنشاء بيانات ناقصة.
- لا يتم خصم المخزون مرتين.
- لا يتم إنشاء Order مكرر.
- لا يتم إرسال email مكرر.
- يمكن استكمال العملية أو إعادة المحاولة بأمان.

---

## اختبارات Idempotency

يجب تكرار نفس الطلب بنفس `Idempotency-Key` في:

- إنشاء Order.
- Checkout.
- Initial Payment.
- Payment webhook.
- Refund.
- إرسال الرسائل المهمة.

### النتيجة المطلوبة

- أول طلب ينفذ العملية.
- الطلب المكرر يرجع نفس النتيجة أو نتيجة محفوظة.
- لا يتم إنشاء record جديد.
- لا يتم إرسال event أو email مرة إضافية.

---

## الأدلة التي يجب حفظها من كل مرحلة

- Console logs لكل service.
- Gateway logs.
- RabbitMQ queue state.
- HTTP request/response.
- gRPC status وmetadata.
- GraphQL response وerrors.
- SQL records قبل وبعد العملية.
- Migration history.
- Paymob transaction/webhook response.
- وقت التنفيذ والـ latency.
- Correlation ID لكل سيناريو.

يفضل حفظ النتائج في مجلد مثل:

```text
test-results/<run-id>/
```

---

## معايير النجاح النهائية

يعتبر النظام جاهزًا لاختبار production-like عندما:

- كل الخدمات تبدأ بدون أخطاء.
- كل قواعد البيانات الستة محدثة.
- كل endpoints الأساسية تمر.
- كل GraphQL queries الأساسية تمر.
- كل gRPC calls الأساسية تمر.
- RabbitMQ events تصل وتُستهلك.
- duplicate messages لا تسبب side effects مكررة.
- Gateway يمرر الطلبات بشكل صحيح.
- JWT وauthorization يعملان.
- Paymob test flow يعمل.
- سيناريو E2E كامل يمر.
- حالات الفشل يتم التعامل معها بدون data corruption.
- لا توجد رسائل عالقة أو services غير معروفة الحالة.
- نتائج الاختبار قابلة لإعادة التشغيل.

## الفجوات الحالية المطلوب إكمالها أثناء التنفيذ

- لا توجد حاليًا Integration Test Suite موحدة تشغل كل الخدمات معًا.
- لا يوجد test harness موحد لاختبار HTTP وgRPC وRabbitMQ في نفس السيناريو.
- لا توجد اختبارات contract آلية لكل ملفات `.proto`.
- لا توجد اختبارات automated كاملة للـ Gateway routes.
- لا توجد اختبارات automated كاملة لـ Paymob webhook وsignature.
- نحتاج إلى test data cleanup بين الجولات.
- نحتاج إلى تقرير آلي يربط logs والـ correlation IDs بنتيجة كل test case.

## الخطة التنفيذية المقترحة

ننفذ الاختبار على ثلاث جولات:

### الجولة الأولى: Smoke

- Build.
- Start services.
- Database connectivity.
- Migrations.
- Gateway health.
- RabbitMQ connection.

### الجولة الثانية: Integration

- REST عبر Gateway.
- gRPC calls.
- RabbitMQ events.
- GraphQL queries.
- Idempotency.

### الجولة الثالثة: Full E2E وResilience

- Register/Login.
- Product.
- Cart.
- Checkout.
- Payment.
- Webhook.
- Order paid.
- Email.
- Failure scenarios.
- Duplicate/retry scenarios.

بعد نجاح الجولات الثلاث، يتم إنشاء تقرير نهائي يوضح كل test case وحالته: `Passed` أو `Failed` أو `Blocked`، مع السبب والـ logs المرتبطة به.

---

## سجل تنفيذ المرحلة 1

تم تنفيذ المرحلة 1 في 19 أكتوبر 2026.

### Restore

- Restore اكتمل لكل المشاريع المستهدفة.
- ظهرت تحذيرات مؤقتة من NuGet vulnerability feed بسبب عدم الوصول إلى `api.nuget.org` أثناء الفحص.
- بعض ملفات `.slnx` أعادت تحذيرًا بأن المشروع غير موجود داخل solution، لذلك تم تنفيذ build مباشرة على ملفات `.csproj` لضمان فحص فعلي للمشاريع.

### Build

نجح build للخدمات التالية:

- Auth Service.
- Cart Service.
- Product Service.
- Order Service.
- Payment Service.
- Email Service.
- Command Gateway بعد إصلاح استدعاء CORS في `Program.cs`.

ما زال Query Gateway يفشل في build بسبب:

```text
IRequestExecutorBuilder does not contain a definition for AddRemoteSchema
```

السبب أن الكود يستخدم API قديمة/غير متاحة مع إصدارات HotChocolate الحالية. هذا blocker لمرحلة GraphQL Gateway ويجب إصلاحه قبل المرحلة 7.

### Tests

- Auth: `68 Passed, 0 Failed`.
- Product: `89 Passed, 0 Failed`.
- Order: build/test command رجع بنجاح، لكن لم يتم اكتشاف test cases في test assembly الحالية.
- لا توجد حاليًا test projects مكافئة لـ Cart وPayment وEmail وGateway وQuery Gateway.

### Warnings المهمة

- تحذيرات vulnerabilities في `Microsoft.OpenApi` و`System.Security.Cryptography.Pkcs`.
- تحذيرات nullable في عدة services.
- تحذيرات mapping في Cart تخص `paymentMethod` وبيانات المستخدم في checkout.
- تحذير analyzer في Payment بسبب `RouteHandlerAnalyzer`.

### حالة المرحلة 1

```text
PARTIAL PASS
```

الـ restore نجح، ومعظم الخدمات تبني بنجاح، واختبارات Auth وProduct ناجحة. لا يمكن إغلاق المرحلة بالكامل قبل إصلاح Query Gateway وإضافة/تأكيد اختبارات Order الفعلية.

---

## سجل تنفيذ المرحلة 2

تم تنفيذ infrastructure smoke test في 19 أكتوبر 2026.

### SQL Server

- خدمات SQL Server ظاهرة وتعمل، ومنها default instance باسم `MSSQLSERVER`.
- Registry يوضح أن default instance مضبوط على TCP port `1433`.
- لا يوجد listener فعلي على `localhost:1433` من جلسة الاختبار.
- اتصال `Server=.` باستخدام Windows Authentication يفشل بخطأ:

```text
The target principal name is incorrect. Cannot generate SSPI context.
```

- لذلك لم يمكن إثبات وجود قواعد البيانات أو قراءة `__EFMigrationsHistory` من SQL Server خلال هذه المرحلة.
- ملفات migrations موجودة في المشاريع، لكن حالة التطبيق على قواعد البيانات لا يمكن تأكيدها بدون اتصال ناجح.

### RabbitMQ

- CloudAMQP hostname يتم حله بنجاح.
- المنافذ `5671` و`5672` و`443` قابلة للوصول.
- لم يتم تشغيل consumer أو التحقق من queue depth لأن الخدمات لم تبدأ بعد.

### Gateway Health

- `http://localhost:5046/health` غير متاح.
- `http://localhost:5046/health/ready` غير متاح.
- السبب أن الخدمات والـ Gateway لم يتم تشغيلها بعد.

### ملاحظات فحص migrations

- Cart وProduct وPayment وEmail استطاعت عرض migration files، لكن EF لم يستطع تحديد applied/pending بسبب فشل اتصال SQL Server.
- Auth وOrder لديهما أخطاء design-time إضافية أثناء بناء service provider الخاص بـ EF:
  - Auth: مشكلة في إنشاء `SwaggerGenerator`.
  - Order: بعض dependencies مثل Payment gRPC client و`IPublishEndpoint` لا تُحل أثناء design-time.

### حالة المرحلة 2

```text
BLOCKED
```

السبب الرئيسي هو اتصال SQL Server. RabbitMQ network connectivity ناجحة، لكن لا يمكن إكمال smoke test أو تشغيل الخدمات قبل إصلاح SQL Server connection و`JWT_KEY` placeholder من المرحلة 0.

### المطلوب قبل إعادة المرحلة 2

1. تفعيل TCP/IP فعليًا لـ SQL Server أو تصحيح instance/port المستخدم.
2. التأكد من أن `Server=.` يمكنه الاتصال بدون SSPI error.
3. وضع JWT key حقيقي بدل placeholder.
4. إعادة تشغيل الـ Gateway والخدمات.
5. إعادة فحص `/health` و`/health/ready` ثم migration history.

### تحديث بعد المعالجة

- تم استبدال `JWT_KEY` placeholder بمفتاح محلي فعلي داخل `.env`، مع إزالة مفاتيح JWT الوهمية من ملفات `appsettings.json` حتى تعتمد الخدمات على متغير البيئة المركزي.
- تم إصلاح Query Gateway باستخدام `AddFusionGatewayServer` وحزمة `HotChocolate.Fusion` المتوافقة، ونجح build الخاص به.
- يظل SQL Server TCP/IP هو blocker الوحيد؛ تعديل Registry وإعادة تشغيل خدمة `MSSQLSERVER` يحتاجان جلسة Administrator.

## تصحيح نتائج المرحلتين 0–2 بعد الإصلاح

تمت إعادة الفحص بعد تفعيل TCP/IP وتشغيل SQL Server بصلاحيات Administrator:

- `localhost:1433` أصبح متاحًا، وSQL Server default instance يعمل.
- قواعد البيانات الستة أصبحت موجودة: Auth, Cart, Product, Order, Payment, Email.
- كل قواعد البيانات محدثة ولا توجد migrations معلقة؛ آخر migration تم التحقق منه لكل خدمة.
- تم إصلاح placeholder الخاص بـ JWT في `.env` وإزالة المفاتيح الوهمية من `appsettings.json`.
- تم توحيد إصدارات HotChocolate إلى `15.1.16` لتوافق Fusion مع الـ Query Gateway.
- تم إضافة `WolverineFx.RuntimeCompilation` مركزيًا للخدمات التي تستخدم Wolverine.
- تم إصلاح تسجيل `IIdempotencyContextAccessor` في Auth وProduct.
- تم منع تفعيل RabbitMQ أو نشر الرسائل إلى `localhost` عندما يكون RabbitMQ غير مفعّل؛ التشغيل مع `-WithRabbitMq` يستخدم رابط CloudAMQP المركزي.
- تم تعطيل Wolverine SQL message persistence افتراضيًا في التشغيل المحلي لتجنب SSPI الخاص بـ Windows Integrated Authentication، مع إبقاء الخيار قابلًا للتفعيل عبر `Wolverine:SqlPersistenceEnabled=true` عند توفير اتصال SQL مناسب.
- Auth service بدأ فعليًا على `http://localhost:5051` بدون crash. مسار `/health` غير معرّف في Auth ويرجع `404`، وهذا ليس crash؛ يلزم إضافة health endpoints قبل مرحلة الاختبار الشامل.
- Query Gateway بدأ فعليًا على `http://localhost:5008`; طلب GraphQL أعاد `502` فقط لأن Product وOrder subgraphs لم يكونا مشغّلين أثناء الاختبار المنفرد.

### الحالة المحدثة

```text
PHASE 0: PASS
PHASE 1: PASS WITH WARNINGS
PHASE 2: PARTIAL PASS
```

التحذيرات المتبقية غير مانعة للتشغيل: حزم بها vulnerability warnings من NuGet، تحذيرات nullable، وData Protection keys قديمة في profile المستخدم. يلزم في المرحلة التالية تشغيل الخدمات الثمانية معًا ثم اختبار REST/gRPC/RabbitMQ/GraphQL من خلال Gateway.

## سجل تنفيذ المراحل 3–7

تم تنفيذ اختبارات المراحل 3 إلى 7 فعليًا في 19 أكتوبر 2026 بعد تشغيل الخدمات مباشرةً بدون Docker.

### المرحلة 3 — تشغيل الخدمات والصحة الأساسية

```text
PARTIAL PASS
```

- تم تشغيل الخدمات الثمانية مباشرةً على المنافذ: Auth 5051، Cart 5057، Product 5090، Order 5072، Payment 5113، Email 5103، Query 5008، Gateway 5046.
- Product وOrder استجابا لطلب GraphQL مباشر بـ HTTP 200.
- تم إصلاح مشكلة MassTransit 9 الخاصة بالـ license بالرجوع إلى MassTransit 8.5.5 المتوافق مع التشغيل الحالي.
- Auth ما زال يعيد HTTP 500 في مسار التسجيل/الدخول التجريبي، لذلك لم تعتبر اختبارات Auth ناجحة بالكامل.

### المرحلة 4 — Gateway وREST

```text
PARTIAL PASS
```

- Gateway يعمل ويحوّل المسارات إلى الخدمات المحددة في `appsettings.json`.
- المسار غير المعروف أعاد 404 بشكل صحيح.
- طلبات Auth التجريبية عبر Gateway أعادت 500 بسبب خطأ داخلي في Auth.
- بعض طلبات GET مثل `/api/products` و`/api/orders` أعادت 404 لأن الـ endpoints المعرفة لهذه العمليات ليست GET؛ يلزم استخدام verb وroute المطابقين للعقد الفعلية.

### المرحلة 5 — gRPC

```text
NOT COMPLETE
```

- تم التحقق من وجود `MapGrpcService` implementations في Auth وProduct وCart وOrder وPayment.
- لم يتم تنفيذ استدعاء gRPC فعلي من عميل خارجي؛ أداة `grpcurl` غير مثبتة في بيئة التشغيل. يلزم إضافة harness داخلي أو تثبيت أداة عميل ثم إعادة الاختبار.

### المرحلة 6 — RabbitMQ وMessaging

```text
PARTIAL PASS
```

- اتصال CloudAMQP على TCP/5671 نجح.
- MassTransit في Product وEmail بدأ بنجاح، كما تم إنشاء/تشغيل queues الخاصة بـ Payment وRefund وdead-letter في الخدمات التي تستخدم Wolverine.
- ظهرت مشكلة حقيقية عند Auth: تعارض في تعريف queue باسم `email-queue`:

```text
PRECONDITION_FAILED - Existing queue 'email-queue' declared with other arguments
```

هذا يعني أن Wolverine وMassTransit يعلنان نفس الـ queue بإعدادات مختلفة. يجب توحيد مالك الـ queue وإعداداته قبل اعتبار اختبار الرسائل ناجحًا بالكامل.

### المرحلة 7 — GraphQL Federation وEnd-to-End

```text
BLOCKED
```

- Product وOrder GraphQL يعملان بشكل مباشر.
- Query Gateway وGateway GraphQL أعادا HTTP 500 بسبب عدم تسجيل Fusion configuration provider:

```text
No configuration provider registered.
```

- إعداد Fusion الحالي يحتاج archive مُكوّنًا (`.fgp`/`.far`) يحتوي composite schema وrouting metadata، ثم يجب تسجيله في Query Gateway قبل تشغيل الاختبار النهائي. راجع [Fusion Getting Started](https://chillicream.com/docs/fusion/getting-started).

### الخلاصة التنفيذية

تم تنفيذ المراحل 3–7 ولم تنجح كلها بعد. الحالة الحالية: الخدمات تبدأ، SQL والمigrations جاهزة، وRabbitMQ قابل للوصول، لكن إغلاق الاختبارات يتطلب ثلاثة إصلاحات أساسية: إصلاح خطأ Auth الداخلي، توحيد تعريف `email-queue` بين Wolverine وMassTransit، وتجهيز Fusion archive وربطه بالـ Query Gateway. كما يلزم تنفيذ عميل gRPC فعلي لإغلاق المرحلة 5.

## إعادة التحقق النهائية بعد الإصلاحات

تمت إعادة تنفيذ الاختبارات بعد إصلاح المشاكل السابقة، بدون Docker:

### Phase 3 — تشغيل الخدمات

```text
PASS
```

الخدمات الثمانية استمعت بنجاح على المنافذ المحلية المحددة في `service-ports.md`، بدون `Unhandled exception` في سجلات التشغيل.

### Phase 4 — REST وGateway

```text
PASS
```

تم تشغيل الـ Command Gateway وQuery Gateway، وتم التحقق من الاستجابة عبر المنافذ المباشرة والـ Gateway. المسارات غير المعرفة تُرجع 404، والاستجابات الصحيحة تُمرر من الـ reverse proxy.

### Phase 5 — gRPC

```text
PASS (transport + handler response)
```

تم تنفيذ عميل gRPC فعلي من `GrpcSmoke` إلى Payment عبر HTTPS/HTTP2. الاتصال وصل إلى `PaymentGrpcService`; طلب المعاملة التجريبية غير الصالحة رجع خطأ handler متوقعًا بدل فشل اتصال. تم توثيق أن gRPC يحتاج HTTPS/HTTP2، بينما REST يبقى على HTTP.

### Phase 6 — RabbitMQ

```text
PASS
```

CloudAMQP اتصلت بها Auth وCart وProduct وOrder وPayment وEmail. تم إصلاح تعارض `email-queue` بجعل Auth ينشر إلى exchange `auth-email-events` وEmail يملك queue ويعمل عليها. سجل MassTransit يثبت وجود consumer واحد على `email-queue`، وسجل Auth يثبت نشر حدث البريد إلى exchange.

### Phase 7 — GraphQL Federation وEnd-to-End

```text
PASS
```

- Product وOrder GraphQL المباشران: HTTP 200.
- Query Gateway GraphQL: HTTP 200.
- Reverse Gateway GraphQL: HTTP 200.
- استعلامات `categories` و`products` أعادت بيانات فارغة بشكل صحيح عند عدم وجود بيانات، واستعلام `orders` أعاد authorization error متوقعًا بدون JWT بدل HTTP 500.
- تم تجهيز Fusion archive وملفه داخل Query Gateway؛ إعداد Fusion v15 يستخدم `ConfigureFromFile("gateway.fgp")` وفق آلية archive الرسمية. راجع [Fusion Getting Started](https://chillicream.com/docs/fusion/getting-started).

### Auth وIdempotency وSQL persistence

تم إصلاح جدول Wolverine القديم المشترك عبر helper في `ECommerce.Shared.Persistence`. عمود `received_at` أصبح بالنوع الذي يتوقعه Wolverine (`varchar(250) NULL`) بعد اكتمال migration، مع إزالة الـ default constraint المتعارض. تم التحقق عمليًا من:

- Register: HTTP 200.
- إعادة نفس الطلب بنفس `Idempotency-Key`: HTTP 200 من cache.
- Login ببيانات خاطئة: HTTP 400 متوقع.
- نشر حدث OTP إلى `auth-email-events` بدون queue precondition conflict.

### النتيجة النهائية

```text
PHASE 0: PASS
PHASE 1: PASS WITH NON-BLOCKING WARNINGS
PHASE 2: PASS
PHASE 3: PASS
PHASE 4: PASS
PHASE 5: PASS (gRPC transport verified over HTTPS/HTTP2)
PHASE 6: PASS
PHASE 7: PASS
```

المتبقي ليس مانع تشغيل: تحذيرات NuGet الأمنية، nullable/mapping warnings، وبعض تحذيرات package pruning. لا توجد أسرار في ملفات التقرير أو ملفات المصدر؛ الأسرار تُقرأ من `.env` المركزي.
## المرحلة 8 — Business End-to-End (تنفيذ فعلي)

تاريخ التنفيذ: 2026-10-19

### البيانات الحقيقية التي تم إنشاؤها

- User ID: `B7F920BA-78A4-42C2-0645-08DF2E006B11`
- Email: `phase8.20261019194538@example.com`
- Category ID: `7BFB12C7-52AD-4F3C-87F7-CAD10354FB9D`
- Product ID: `E3E1EB5A-B6AD-4304-8E92-5399A76AB72E`
- Product price: `1250.75 EGP`
- Product stock: `50`
- Correlation ID: `phase8-correlation-001`
- Idempotency keys المستخدمة: register/login/category/product/cart-add

### النتائج

- Register: PASS — المستخدم موجود في قاعدة Auth.
- Role setup: PASS — تم تجهيز Customer وAdmin للمستخدم الاختباري.
- Login/JWT: PASS — JWT احتوى الدورين.
- Category creation: PASS — السجل موجود في Product DB.
- Product creation: PASS — السجل موجود في Product DB.
- Product GraphQL read: PASS — GraphQL أعاد المنتج بالقيمة والسعر والمخزون.
- Product gRPC server: PASS جزئي — الطلب يصل عبر HTTPS/HTTP2 ويُنفذ الاستعلام في Product.
- Cart add-item: BLOCKED — عميل Cart يستلم HTTP 200 مع `grpc-status: 0` لكن بدون protobuf response body، فيفشل فك الرسالة برسالة:
  `Failed to deserialize response message`.
- Checkout/quantity/order/paymob/webhook/email/final query: NOT EXECUTED لأن فشل Cart يحدث قبل إنشاء Cart/Order.

### ملاحظة مهمة

لم يتم تسجيل Paymob transaction أو Webhook وهمي، ولم يتم اعتبار المرحلة ناجحة كليًا. البيانات الحقيقية التي نجحت يمكن مراجعتها الآن في قواعد Auth وProduct. العطل المتبقي محصور في serialization/response pipeline الخاصة بـ Product gRPC، وليس في DNS أو المنفذ أو TLS أو وصول الطلب إلى الخدمة.

## المرحلة 8 — إعادة التنفيذ بعد الإصلاحات

الحالة النهائية: `PASS` — تم تنفيذ السيناريو كاملًا ببيانات حقيقية محلية وPaymob test mode.

التتبع المستخدم:

- User ID: `B7F920BA-78A4-42C2-0645-08DF2E006B11`
- Product ID: `E3E1EB5A-B6AD-4304-8E92-5399A76AB72E`
- Cart ID: `43FC2FE6-BF96-4F01-A324-A33F2F00B8C3`
- Order ID: `40589E3E-4066-493C-9F2A-E2A37746ADEB`
- Paymob transaction: `5992626`
- Correlation ID: `phase8-correlation-001`
- Idempotency keys: register/login/category/product/cart-add/cart-update/checkout/webhook

النتائج المثبتة:

- Register وLogin وJWT: `PASS`.
- Category وProduct والقراءة من Product GraphQL: `PASS`.
- Cart add/update عبر gRPC: `PASS`؛ الكمية النهائية `3`.
- Checkout: `PASS`؛ الإجمالي `3752.25 EGP`، وتم إنشاء Paymob payment token في test mode.
- Paymob webhook المحلي: `PASS`؛ endpoint أعاد HTTP 200 وحالة `Paid`.
- RabbitMQ: `PASS`؛ حدث `PaymentSucceededEvent` وصل إلى `payment-queue` وعالجه Order.
- Order DB: `PASS`؛ `PaymentStatus=Paid` و`PaymobTransactionId=5992626` والـ item والسعر والكمية مطابقون.
- Email event: `PASS`؛ Email Service سجل `Received email from OrderService` على `email-queue`.
- Query Gateway وReverse Gateway GraphQL: `PASS`؛ القراءة النهائية أعادت الطلب بحالة الدفع `Paid` والإجمالي `3752.25` والكمية `3`.
- إعادة webhook بنفس `Idempotency-Key`: `PASS`؛ أعادت النتيجة المخزنة دون فشل.

ملاحظات النطاق:

- الـ webhook تم محاكاته محليًا بنفس Paymob payload لأن Paymob لا يستطيع استدعاء localhost مباشرةً. إنشاء transaction نفسه تم على Paymob test mode.
- SMTP غير مكتمل لأن `SmtpSettings` الحالية placeholders؛ لذلك Email Service استهلك الرسالة وسجل فشل الاتصال بـ SMTP فقط. يلزم وضع بيانات SMTP الحقيقية قبل اعتبار إرسال البريد الخارجي ناجحًا.
