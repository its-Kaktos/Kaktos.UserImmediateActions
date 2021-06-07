<div dir="ltr">

# A package to perform immediate log-out and refresh-sign-in on users.

This project adds functionality on top of the asp.net core Identity package to support immediate log-out and refresh-sign-in.

### [برای دیدن داکیومنت فارسی اینجا رو کلیک کنید](#یک-پکیج-برای-انجام-عملیات-های-log-out-و-refresh-signin-به-صورت-آنی-بر-روی-کاربران)

## Use cases :

To immediately remove/add a role or a claim from/to a user as an admin, you can first remove the roles you want using `UserMnager` and `RoleManager` classes and then use the refresh-sign in functionality to
immediately update the user cookie. To sign out a user from their account you can use the sign-out functionality to sign out a user immediately from their account.

## Quick access

- [Quick tutorial](#quick-tutorial)
- [How to use IUserImmediateActionsService](#how-to-use-iuserimmediateactionsservice-)
- [Services](#services)
    - [IDateTimeProvider](#idatetimeprovider-)
    - [IImmediateActionsStore](#iimmediateactionsstore-)
    - [IPermanentImmediateActionsStore](#ipermanentimmediateactionsstore-)
    - [IUserActionStoreKeyGenerator](#iuseractionstorekeygenerator-)
    - [IUserImmediateActionsService](#iuserimmediateactionsservice-)
    - [ICurrentUserWrapperService](#icurrentuserwrapperservice-)
- [Extension methods](#extension-methods)
    - [AddUserImmediateActions](#adduserimmediateactions-)
    - [AddPermanentImmediateActionsStore](#addpermanentimmediateactionsstore-)
    - [AddDefaultDistributedImmediateActionStore](#adddefaultdistributedimmediateactionstore-)
    - [AddDistributedImmediateActionStore](#adddistributedimmediateactionstore-)
    - [AddUserActionStoreKeyGenerator](#adduseractionstorekeygenerator-)
    - [AddUserImmediateActionsService](#adduserimmediateactionsservice-)
    - [AddCurrentUserWrapperService](#addcurrentuserwrapperservice-)
- [How this package works](#how-this-package-works-)

## Quick tutorial

1. To start, after using the `AddIdentity` and setting up the identity services, use [`AddUserImmediateActions`](#adduserimmediateactions-) extension method to add all the [default services](#services) to IoC container.

```c#
services.AddIdentity<IdentityUser, IdentityRole>() 
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddUserImmediateActions();
```

<br/>

2. After registering the services, its now time for some Middlewares! use the `UseUserImmediateActions` middleware as shown below. make sure to use it between
   the `UseAuthentication` and `UseAuthorization` for correct behavior.

```c#
app.UseAuthentication();

app.UseUserImmediateActions();

app.UseAuthorization();
```

3. Now that we registered all the services and Middlewares that we need (that was quick!) it's now time to use this package. The main service that you need to use
   is [`IUserImmediateActionsService`](#iuserimmediateactionsservice-).

```c#
public class MyController : Controller
{
    private readonly IUserImmediateActionsService _userImmediateActionsService;

    public MyController(IUserImmediateActionsService userImmediateActionsService)
    {
        _userImmediateActionsService = userImmediateActionsService;
    }
}
```

Now get an instance of [`IUserImmediateActionsService`](#iuserimmediateactionsservice-) from the IoC container using the constructor injection (or any other method you want), and then use it to perform your desired
action!

```c#
[HttpPost]
public async Task<IActionResult> EditUser(EditUser model)
{
    if (ModelState.IsValid)
    {
        /* 
            First, update the user, then use the method below for user immediate cookie update AKA refresh-sign in.
        */
        await _userImmediateActionsService.RefreshCookieAsync(model.UserId);

        /* 
            If for any reason you want to sign a user out from their account, use the method below.
            Also, it is recommended to update the user SecurityStamp too so we can ensure a sign-out on all devices,
            to update the user SecurityStamp, use the second method below.
        */
        await _userImmediateActionsService.SignOutAsync(model.UserId);
        await _userManager.UpdateSecurityStampAsync(await _userManager.FindByIdAsync(model.UserId));
    }

    return View(model);
}

```

**Highlight:**
Its recommended reading the [documentation](#documentation) as it contains useful information about how this package works.

## Documentation

## How to use `IUserImmediateActionsService` :

( This part makes more sense if you have read the [services](#services) section first. )

In this service, there are two main methods `SignOut` and `RefreshCookie`, and also their Async version. Both of them will get an UserId as a parameter and will turn it into a
unique key before storing it using the **main store service** alongside some other information. In the Async methods the storing operation is done asynchronously (depends on the
provided implementation of [`IImmediateActionsStore`](#iimmediateactionsstore-)).

**Warning:**
Make sure to update user SecurityStamp using the `UpdateSecurityStampAsync` method from `UserManager` when calling the `SignOut` or `SingOutAsync`, to make sure the user will be
singed-out from their account on all their devices. if SecurityStamp is not updated, there is a possibility that the user will not be signed out on some devices.

## Services

### `IDateTimeProvider` :

This service is used internally to increase the testability of this package when using DateTime.Now.

### `IImmediateActionsStore` :

`IImmediateActionsStore` service is all about storing data. in this package we have two types of storage, first type is `IImmediateActionsStore` and we will call it the **main
store service** in this documentation. The main pro of this service is its performance, it can save and especially read data pretty fast. for this reason, this service will use some
sort of caching mechanism to store data. By default, this service will use
`IMemoryCache` for storing data, but you can change it to use `IDistributedCache` by using the [`AddDefaultDistributedImmediateActionStore`](#adddefaultdistributedimmediateactionstore-)
extension method.

### `IPermanentImmediateActionsStore` :

`IPermanentImmediateActionsStore` service is the second type of storage, and we will call it the **permanent store service**
in this documentation. The main pro of this service is that it can save data permanently. We can use this server to avoid losing data on each application restart because of the use
of caching services. Internally when the [**main store service**](#iimmediateactionsstore-)
methods are called to store some data, the **permanent store service** methods are called to store the same data.

**Warning:**
By default there is no real implementation of **permanent store service**, the added implementation of this service is fake and does nothing. To register a real working
implementation of this service you need to use the [`AddPermanentImmediateActionsStore`](#--addpermanentimmediateactionsstore-)
and pass your implementation to it.

### `IUserActionStoreKeyGenerator` :

`IUserActionStoreKeyGenerator` service is responsible for generating the unique keys that is used in the [**main store service**](#iimmediateactionsstore-).

### `IUserImmediateActionsService` :

`IUserImmediateActionsService` service contains the methods that you will use. explained in the [quick tutorial](#quick-tutorial).

### `ICurrentUserWrapperService` :

`ICurrentUserWrapperService` service is used internally in this package. it is used when a user needs to be singed-out
or we need to refresh their cookie. By default, the implementation of this service uses `UserManager` and `SignInManager`
provided by the asp.net core Identity package to achieve its goals. to use your own implementation use
[`AddCurrentUserWrapperService`](#addcurrentuserwrapperservice-) extension method and pass your implementation to it.

## Extension methods

### `AddUserImmediateActions` :

Adds the default services to the IoC container. you can read more about the provided services in the services section.

```c#
services.AddMemoryCache();
services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
services.TryAddSingleton<IPermanentImmediateActionsStore, FakePermanentImmediateActionsStore>();
services.TryAddTransient<IImmediateActionsStore, MemoryCacheImmediateActionsStore>();
services.TryAddSingleton<IUserActionStoreKeyGenerator, UserActionStoreKeyGenerator>();
services.TryAddTransient<IUserImmediateActionsService, UserImmediateActionsService>();
services.TryAddScoped(typeof(ICurrentUserWrapperService), typeof(IdentityCurrentUserWrapperService<>).MakeGenericType(builder.UserType));
```

### `AddPermanentImmediateActionsStore` :

Adds a [**permanent store service**](#ipermanentimmediateactionsstore-) implementation.( Require inheritance from `IPermanentImmediateActionsStore` )

```c#
public static IdentityBuilder AddPermanentImmediateActionsStore<TPermanentStore>(this IdentityBuilder builder) where TPermanentStore : class, IPermanentImmediateActionsStor
```

### `AddDefaultDistributedImmediateActionStore` :

Adds the default implementation of [**main store service**](#iimmediateactionsstore-) that uses `IDistributedCache`.

```c#
public static IdentityBuilder AddDefaultDistributedImmediateActionStore(this IdentityBuilder builder)
```

### `AddDistributedImmediateActionStore` :

Adds your implementation of [**main store service**](#iimmediateactionsstore-).( Require inheritance from `IImmediateActionsStore` )

```c#
public static IdentityBuilder AddDistributedImmediateActionStore<TStore>(this IdentityBuilder builder) where TStore : class, IImmediateActionsStore
```

### `AddUserActionStoreKeyGenerator` :

Adds your implementation for generating unique keys that are used in the [**main store service**](#iimmediateactionsstore-).( Require inheritance from `IUserActionStoreKeyGenerator` )

```c#
public static IdentityBuilder AddUserActionStoreKeyGenerator<TGenerator>(this IdentityBuilder builder) where TGenerator : class, IUserActionStoreKeyGenerator
```

### `AddUserImmediateActionsService` :

Adds your implementation of [`IUserImmediateActionsService`](#iuserimmediateactionsservice-) that contains the methods you will use!( Require inheritance from `IUserImmediateActionsService` )

```c#
public static IdentityBuilder AddUserImmediateActionsService<TActionService>(this IdentityBuilder builder) where TActionService : class, IUserImmediateActionsService
```

### `AddCurrentUserWrapperService` :

Adds your implementation of [`ICurrentUserWrapperService`](#icurrentuserwrapperservice-) that contains the methods used to refresh and sign out a user 
( this service is used internally in this package ). ( Require inheritance from `ICurrentUserWrapperService` )

```c#
public static IdentityBuilder AddCurrentUserWrapperService<TUserWrapperService>(this IdentityBuilder builder) where TUserWrapperService : class, ICurrentUserWrapperService
```

## How this package works :

( This part makes more sense if you have read the [services](#services) section first. )

After calling one of the methods in [`IUserImmediateActionsService`](#iuserimmediateactionsservice-), the given userId will be used to generate a unique key using
[`IUserActionStoreKeyGenerator`](#iuseractionstorekeygenerator-). After that an instance of `ImmediateActionDataModel` that contains the generated key and some
other information will be stored in the [**main store**](#iimmediateactionsstore-) (and possibly the [**permanent store**](#ipermanentimmediateactionsstore-) if an implementation is provided),
then in the `UserImmediateActionsMiddleware` we will check every user if they need to be signed-out or their cookie needs to be refreshed using the data given by [**main store service**](#iimmediateactionsstore-). It is worth mentioning that because of how this middleware is implemented and the usage of a caching service that usually stores data in the ram, there should be little to no performance penalty. 
(The main bottleneck here is the read performance of the caching service used and we can't really benchmark it,
because the performance will vary based on the caching service you use, your latency to the caching server, and many other parameters. but as mentioned, generally, there should be little to
no performance penalty.)

## Give a Star! ⭐️

If you like this project or you are using it in your application, please give it a star. Thanks!

</div>

---

<div dir="rtl">

# یک پکیج برای انجام عملیات های log-out و refresh-signin به صورت آنی بر روی کاربران.

این پکیج به سیستم asp.net core Identity قابلیت sign-out و refresh-signin به صورت آنی را اضافه خواهد کرد.

## موارد استفاده :

برای حذف کردن یک Role یا Claim به صورت آنی میتوان از refresh-signin استفاده کرد تا مقادیر آپدیت شده به صورت آنی در کوکی کاربر اعمال شوند. برای خارج کردن کاربر از حساب کاربری خود به
صورت آنی، میتوان از sign-out استفاده کرد تا کاربر به صورت آنی از حساب کاربری خود خارج شود.

## دسترسی سریع

- [آموزش استفاده](#آموزش-استفاده)
- [آموزش استفاده کامل از IUserImmediateActionsService](#آموزش-استفاده-کامل-از-iuserimmediateactionsservice-)
- [سرویس ها](#سرویس-ها)
    - [سرویس IDateTimeProvider](#---idatetimeprovider-)
    - [سرویس IImmediateActionsStore](#---IImmediateActionsStore-)
    - [سرویس IPermanentImmediateActionsStore](#---IPermanentImmediateActionsStore-)
    - [سرویس IUserActionStoreKeyGenerator](#---IUserActionStoreKeyGenerator-)
    - [سرویس IUserImmediateActionsService](#---IUserImmediateActionsService-)
    - [سرویس ICurrentUserWrapperService](#---ICurrentUserWrapperService-)
- [اکستنشن متود ها](#اکستنشن-متود-ها)
    - [اکستنشن متود AddUserImmediateActions](#اکستنشن-متود-adduserimmediateactions-)
    - [اکستنشن متود AddPermanentImmediateActionsStore](#اکستنشن-متود-AddPermanentImmediateActionsStore-)
    - [اکستنشن متود AddDefaultDistributedImmediateActionStore](#اکستنشن-متود-AddDefaultDistributedImmediateActionStore-)
    - [اکستنشن متود AddDistributedImmediateActionStore](#اکستنشن-متود-AddDistributedImmediateActionStore-)
    - [اکستنشن متود AddUserActionStoreKeyGenerator](#اکستنشن-متود-AddUserActionStoreKeyGenerator-)
    - [اکستنشن متود AddUserImmediateActionsService](#اکستنشن-متود-AddUserImmediateActionsService-)
    - [اکستنشن متود AddCurrentUserWrapperService](#اکستنشن-متود-AddCurrentUserWrapperService-)
- [نحوه کار این پکیج](#نحوه-کار-این-پکیج-)

## آموزش استفاده

1. برای شروع بعد از استفاده از `AddIdentity` و موارد موردنیاز خود، از اکستنشن متود [`AddUserImmediateActions`](#اکستنشن-متود-adduserimmediateactions-) استفاده بکنید تا تمامی سرویس های پیشفرض موردنیاز به سیستم Dependency
   injection اضافه بشوند.

<div dir="ltr">

```c#
services.AddIdentity<IdentityUser, IdentityRole>() 
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddUserImmediateActions();
```

</div>
<br/>

2. بعد از اضافه کردن سرویس های مورد نیاز، نوبت به اضافه کردن Middleware میرسه! از `UseUserImmediateActions` به صورت زیر استفاده کنید. دقت داشته باشید که `UseUserImmediateActions`
   حتما بین `UseAuthentication` و `UseAuthorization` باشه تا به درستی کار بکنه.

<div dir="ltr">

```c#
app.UseAuthentication();

app.UseUserImmediateActions();

app.UseAuthorization();
```

</div>

3. خوب الان که تمامی سرویس ها و Middleware های موردنیاز رو اضافه کردیم ( چه سریع تموم شد! ) نوبت به استفاده از این سرویس ها میرسه. سرویس اصلی که شما با اون سروکار خواهید داشت
   سرویس [`IUserImmediateActionsService`](#---iuserimmediateactionsservice-) خواهد بود.

<div dir="ltr">

```c#
public class MyController : Controller
{
    private readonly IUserImmediateActionsService _userImmediateActionsService;

    public MyController(IUserImmediateActionsService userImmediateActionsService)
    {
        _userImmediateActionsService = userImmediateActionsService;
    }
}
```

</div>

از طریق سازنده کلاس ( Constroctur ) یک نمونه از سرویس  [`IUserImmediateActionsService`](#---iuserimmediateactionsservice-) دریافت کنید و سپس از ان برای انجام عملیات موردنظر خود استفاده کنید!
<div dir="ltr">

```c#
[HttpPost]
public async Task<IActionResult> EditUser(EditUser model)
{
    if (ModelState.IsValid)
    {
        /* 
            ابتدا کاربر را آپدیت کنید و سپس از متود زیر برای آپدیت آنی کاربر استفاده کنید   
        */
        await _userImmediateActionsService.RefreshCookieAsync(model.UserId);

        /* 
        اگر به هر دلیلی میخواهید کاربر را از حساب کاربری خود
خارج کنید، از متود زیر استفاده کنید. همچنین پیشنهاد میشود
SecurityStamp کاربر رو هم از طریق متود زیر آپدیت کنید
که مطمئن شویم کاربر از حساب خود خارج میشود 
        */
        await _userImmediateActionsService.SignOutAsync(model.UserId);
        await _userManager.UpdateSecurityStampAsync(await _userManager.FindByIdAsync(model.UserId));
    }

    return View(model);
}

```

</div>

**نکته :**
پیشنهاد میشود موارد مربوط به [داکیومنت](#داکیومنت) حتما خوانده شود.

## داکیومنت

## آموزش استفاده کامل از `IUserImmediateActionsService` :

( پیشنهاد میشود قبل ازخواندن این متن، با مطالب ارائه شده در قسمت [سرویس ها](#سرویس-ها) آشنا باشید )

در این سرویس دو متود وجود دارد `RefreshCookie` و `SignOut`. که در هر دو این متود ها یک متود Async هم دارند و همچنین هر دو یک ای دی کاربر به عنوان پارامتر دریافت میکنند. این پارامتر
ای دی ابتدا تبدیل یه یک کلید منحصر به فرد میشود و سپس به سرویس **ذخیره سازی اصلی**  برای ذخیره کردن کلید ( ایدی کاربر ) به همراه بعضی اطلاعات مورد نیاز پاس داده میشود. در متود های
Async عملیات ذخیره سازی به صورت Async انجام خواهد شد.

**اخطار :**
پیشنهاد میشود در هنگام استفاده از متود `SignOut` و یا `SingOutAsync`، حتما `SecurityStamp` کاربر رو هم با استفاده از متود `UpdateSecurityStampAsync` موجود در کلاس `UserManager` (
پکیج Identity) آپدیت بکنید، که مطمئن شویم کاربر در تمامی دستگاه های خودش از حساب کاربری اش خارج میشود. اگر این کار انجام نشود احتمال خارج نشدن کاربر از حساب کاربری اش در بعضی از
دستگاه ها وجود دارد.

## سرویس ها

### ● - `IDateTimeProvider` :

از این سرویس به صورت توکار برای بالا بردن قابلیت تست پذیری کدها استفاده میشود.

### ● - `IImmediateActionsStore` :

سرویس `IImmediateActionsStore` مربوط به سیستم ذخیره سازی هستش. در این پکیج دو نوع سیستم ذخیره سازی استفاده شده است، اولین نوع سرویس `IImmediateActionsStore` هستش که از این به بعد
اون رو با نام **سرویس ذخیره سازی اصلی** میشناسیم. مشخصه اصلی این سرویس ذخیره سازی، سرعت و پرفورمنس اون هستش، یعنی این سرویس باید سرعت بالایی در ذخیره سازی و مخصوصا خوندن اطلاعات
داشته باشه. به همین دلیل محل ذخیره سازی اطلاعات در این سرویس، یکی از انواع مختلف سیستم های Cache میباشد. به صورت پیشفرض از `IMemoryCache` برای ذخیره سازی اطلاعات در این سرویس
استفاده میشود که میتوان آن را با `IDistributedCache` جایگزین کرد، با استفاده از اکستنشن متود [`AddDefaultDistributedImmediateActionStore`](#اکستنشن-متود-adddefaultdistributedimmediateactionstore-).

### ● - `IPermanentImmediateActionsStore` :

سرویس `IPermanentImmediateActionsStore` نوع دوم سیستم ذخیره سازی اطلاعات در این پکیج هستش، که از این به بعد اون رو با نام **سرویس ذخیره سازی دائمی** میشناسیم. مشخصه اصلی این سرویس
ذخیره سازی دائمی اطلاعات هستش. از این سرویس برای جلوگیری از حذف شدن اطلاعات در زمان ریستارت شدن اپلیکیشن استفاده میشود. به صورت توکار هنگاه صدا زده شدن متود های سرویس **ذخیره ساز
اصلی**، متود های سرویس [**ذخیره ساز دائمی**](#---iimmediateactionsstore-) هم صدا زده میشوند تا اطلاعات در این سرویس نیز ذخیره بشوند.

**نکته مهم** :
به صورت پیشفرض سرویس **ذخیره ساز دائمی** به صورت فیک پیاده سازی( Implement ) شده و به `IServiceCollection` اضافه میشود. برای اضافه کردن سرویس **ذخیره ساز دائمی** واقعی نیاز دارید
که از اکستنشن متود [`AddPermanentImmediateActionsStore`](#اکستنشن-متود-addpermanentimmediateactionsstore-) استفاده کنید و سرویس اصلی خود که پیاده سازی کرده اید را بهش پاس بدهید.

### ● - `IUserActionStoreKeyGenerator` :

سرویس `IUserActionStoreKeyGenerator` مربوط به تولید کلید های منحصر به فرد برای استفاده در سرویس  [**ذخیره سازی اصلی**](#---iimmediateactionsstore-) مورد استفاده قرار میگیره.

### ● - `IUserImmediateActionsService` :

سرویس `IUserImmediateActionsService` مربوط به متود هایی هستش که توسط شما مورد استفاده قرار میگیره. که در قسمت [آموزش استفاده](#آموزش-استفاده) توضیح داده شده است

### ● - `ICurrentUserWrapperService` :

سرویس `ICurrentUserWrapperService` به صورت توکار در پکیج استفاده میشود. از این سرویس در زمانی که کاربر نیاز به خارج شدن از حساب کاربری خود داره یا نیاز به refresh-signin اون هستش
استفاده میشه. به صورت پیشفرض از کلاس های `UserManager` و `SignInManager` ( مربوط به پکیج Identity ) برای اینکار استفاده میشود. اگر نیاز دارید که سرویس شخصی خودتون رو اضافه کنید
میتونید از اکستنشن متود [`AddCurrentUserWrapperService`](#اکستنشن-متود-addcurrentuserwrapperservice-) استفاده کنید و سرویس خودتون رو بهش پاس بدید.

## اکستنشن متود ها

### اکستنشن متود `AddUserImmediateActions` :

بعد از استفاده از اکستنشن متود `AddUserImmediateActions` سرویس های زیر به `IServiceCollection` اضافه میشوند. اطلاعات بیشتر در مورد سرویس های اضافه شده رو میتونید در قسمت [سرویس ها](#سرویس-ها)
بخونید.
<div dir="ltr">

```c#
services.AddMemoryCache();
services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
services.TryAddSingleton<IPermanentImmediateActionsStore, FakePermanentImmediateActionsStore>();
services.TryAddTransient<IImmediateActionsStore, MemoryCacheImmediateActionsStore>();
services.TryAddSingleton<IUserActionStoreKeyGenerator, UserActionStoreKeyGenerator>();
services.TryAddTransient<IUserImmediateActionsService, UserImmediateActionsService>();
services.TryAddScoped(typeof(ICurrentUserWrapperService), typeof(IdentityCurrentUserWrapperService<>).MakeGenericType(builder.UserType));
```

</div>

### اکستنشن متود `AddPermanentImmediateActionsStore` :

اضافه کردن یک [**ذخیره سازدائمی**](#---ipermanentimmediateactionsstore-). ( نیازمند ارث برای از اینترفیس [`IPermanentImmediateActionsStore`](#---ipermanentimmediateactionsstore-) )
<div dir="ltr">

```c#
public static IdentityBuilder AddPermanentImmediateActionsStore<TPermanentStore>(this IdentityBuilder builder) where TPermanentStore : class, IPermanentImmediateActionsStor
```

</div>

### اکستنشن متود `AddDefaultDistributedImmediateActionStore` :

اضافه کردن پیاده سازی (Implementation) پیشفرض برای استفاده از `IDistributedCache` به عنوان سرویس [**ذخیره سازی اصلی**](#---iimmediateactionsstore-).
<div dir="ltr">

```c#
public static IdentityBuilder AddDefaultDistributedImmediateActionStore(this IdentityBuilder builder)
```

</div>

### اکستنشن متود `AddDistributedImmediateActionStore` :

اضافه کردن سرویس شخصی و کاستومایز شده برای [**ذخیره ساز اصلی**](#---iimmediateactionsstore-). ( نیازمند ارث برای از اینترفیس [`IImmediateActionsStore`](#---iimmediateactionsstore-) )
<div dir="ltr">

```c#
public static IdentityBuilder AddDistributedImmediateActionStore<TStore>(this IdentityBuilder builder) where TStore : class, IImmediateActionsStore
```

</div>

### اکستنشن متود `AddUserActionStoreKeyGenerator` :

اضافه کردن سرویس شخصی و کاستومایز شده برای تولید کلید های مورد استفاده در سرویس [**ذخیره ساز اصلی**](#---iimmediateactionsstore-). ( نیازمند ارث برای از اینترفیس [`IUserActionStoreKeyGenerator`](#---iuseractionstorekeygenerator-) )
<div dir="ltr">

```c#
public static IdentityBuilder AddUserActionStoreKeyGenerator<TGenerator>(this IdentityBuilder builder) where TGenerator : class, IUserActionStoreKeyGenerator
```

</div>

### اکستنشن متود `AddUserImmediateActionsService` :

اضافه کردن سرویس شخصی و کاستومایز شده برای انجام عملیات های موردنظر برروی کاربر. ( نیازمند ارث برای از اینترفیس  [`IUserImmediateActionsService`](#---iuserimmediateactionsservice-) )
<div dir="ltr">

```c#
public static IdentityBuilder AddUserImmediateActionsService<TActionService>(this IdentityBuilder builder) where TActionService : class, IUserImmediateActionsService
```

</div>

### اکستنشن متود `AddCurrentUserWrapperService` :

اضافه کردن سرویس شخصی و کاستومایز شده برای گرفتن اطلاعات و انجام عملیات بر روی کاربر ( این سرویس به صورت توکار در پکیج استفاده میشود ). ( نیازمند ارث برای از
اینترفیس [`ICurrentUserWrapperService`](#---icurrentuserwrapperservice-) )
<div dir="ltr">

```c#
public static IdentityBuilder AddCurrentUserWrapperService<TUserWrapperService>(this IdentityBuilder builder) where TUserWrapperService : class, ICurrentUserWrapperService
```

</div>

## نحوه کار این پکیج :

( پیشنهاد میشود قبل ازخواندن این متن، با مطالب ارائه شده در قسمت [سرویس ها](#سرویس-ها) آشنا باشید )

بعد از صدا زدن یکی از متود های سرویس  [`IUserImmediateActionsService`](#---iuserimmediateactionsservice-)، ای دی کاربر پاس داده شده به متود موردنظر به یک کلید تبدیل میشود، با استفاده از `IUserActionStoreKeyGenerator`.
بعد از آن یک مدل `ImmediateActionDataModel` به سرویس [**ذخیره سازی اصلی**](#---iimmediateactionsstore-) ( و [**ذخیره سازی دائمی**](#---ipermanentimmediateactionsstore-)، اگر اضافه شده باشد ) اضافه خواهد شد که این مدل اضافه شده حاوی اطلاعات مورد نیاز
برای انجام عملیات روی کاربر مورد نظر میباشد. سپس در `UserImmediateActionsMiddleware` بررسی میشود که اگر کاربری که Request فرستاده است، اگر ای دی کاربری اش در سرویس **ذخیره سازی
اصلی** وجود داشته باشد عملیات موردنظر (مثلا SignOut)
برای او انجام میشود.

## ستاره بدید! ⭐️

اگر از این پروژه خوشتون اومده یا دارید ازش در اپلیکیشن خود استفاده میکنید، لطفا به این ریپو یک ستاره بدید. ممنون!

</div>

