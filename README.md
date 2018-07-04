# CmsZwo
A powerful framework for cms and application servers.

## CmsZwo.Container
A Lightweight IoC Container with Property Dependency Injection. This library helps developing loosely coupled applications.

### Conventions
- Services in IoC container are kept as singleton
- Instanced injection is coming later
- There will be no configurable or named instances

### IoC Container
The Container is kept very simple and lightweight.

### Injector
Only supports public property injection. All Services must have IService interface and support new().

### Injection
- The order of registration does not matter
- Injection is applied only on first resolve
- Services is registred with all interfaces derived from IService as alias
- Supports circle dependencies
- Supports nested dependencies

## CmsZwo.Cache
A powerfull InProc memory cache with Redis support for distributed caching.

### DynamicList
A powerful concept to agressively and effectively cache entities.