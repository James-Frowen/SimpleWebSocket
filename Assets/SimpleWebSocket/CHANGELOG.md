## [4.1.5](https://github.com/James-Frowen/SimpleWebSocket/compare/v4.1.4...v4.1.5) (2024-10-01)


### Bug Fixes

* updating SWT to version 2.2.2 ([9bc3797](https://github.com/James-Frowen/SimpleWebSocket/commit/9bc3797222a0dc875d31aad7e3a3074a1d1d8efc))

## [4.1.4](https://github.com/James-Frowen/SimpleWebSocket/compare/v4.1.3...v4.1.4) (2024-09-15)


### Bug Fixes

* adding inspector header ([f28f162](https://github.com/James-Frowen/SimpleWebSocket/commit/f28f1622aa981805359cbcd9264ad17f01e2db24))

## [4.1.3](https://github.com/James-Frowen/SimpleWebSocket/compare/v4.1.2...v4.1.3) (2024-09-15)


### Bug Fixes

* adding tooltips and default values for TcpConfig ([7b95410](https://github.com/James-Frowen/SimpleWebSocket/commit/7b95410a9be51e1e88fe00e12adec82ce7ce7706))

## [4.1.2](https://github.com/James-Frowen/SimpleWebSocket/compare/v4.1.1...v4.1.2) (2024-09-15)


### Bug Fixes

* fixing TcpConfig now showing in inspector ([5d19d2c](https://github.com/James-Frowen/SimpleWebSocket/commit/5d19d2ccba4409318aa65381857df029f275eb37))

## [4.1.1](https://github.com/James-Frowen/SimpleWebSocket/compare/v4.1.0...v4.1.1) (2024-07-08)


### Bug Fixes

* adding field for max handshake size ([93b6c52](https://github.com/James-Frowen/SimpleWebSocket/commit/93b6c52ffd797c84b0c1328a39dba24c86f41e49))

# [4.1.0](https://github.com/James-Frowen/SimpleWebSocket/compare/v4.0.1...v4.1.0) (2024-04-09)


### Features

* adding option to allow ssl errors. Useful when testing with self signed cert ([46cf1fd](https://github.com/James-Frowen/SimpleWebSocket/commit/46cf1fdc14db44a741a22ffc1187399bb4749c17))

## [4.0.1](https://github.com/James-Frowen/SimpleWebSocket/compare/v4.0.0...v4.0.1) (2024-03-16)


### Bug Fixes

* updating unity version in package ([2ece3e6](https://github.com/James-Frowen/SimpleWebSocket/commit/2ece3e64592ab51481fa93e7031959b9deb3789d))

# [4.0.0](https://github.com/James-Frowen/SimpleWebSocket/compare/v3.1.2...v4.0.0) (2024-03-16)


* fix!: SWT version to fix Runtime in global scope ([76ea692](https://github.com/James-Frowen/SimpleWebSocket/commit/76ea692f2551784671a58af076280867ebc6e900))


### BREAKING CHANGES

* no longer supports unity 2020 or earlier

## [3.1.2](https://github.com/James-Frowen/SimpleWebSocket/compare/v3.1.1...v3.1.2) (2024-03-14)


### Bug Fixes

* updating SimpleWebTransport to version 1.6.5 for fixes ([585369e](https://github.com/James-Frowen/SimpleWebSocket/commit/585369ee1b23980a00617fc314b29b89dc039da3))

## [3.1.1](https://github.com/James-Frowen/SimpleWebSocket/compare/v3.1.0...v3.1.1) (2023-12-05)


### Bug Fixes

* fixing use of new client port ([3fdfb1b](https://github.com/James-Frowen/SimpleWebSocket/commit/3fdfb1b55c4bd702708b21b2f0e1315b0eec905a))

# [3.1.0](https://github.com/James-Frowen/SimpleWebSocket/compare/v3.0.0...v3.1.0) (2023-12-05)


### Features

* adding client port settings ([39cfaa0](https://github.com/James-Frowen/SimpleWebSocket/commit/39cfaa01c748a4b40138d532002f88501b41a074))

# [3.0.0](https://github.com/James-Frowen/SimpleWebSocket/compare/v2.0.3...v3.0.0) (2023-04-02)


* fix!: allowing uri to be used by endpoint ([4dfff2d](https://github.com/James-Frowen/SimpleWebSocket/commit/4dfff2d096f35c5293f75cdc10c4e5ecf895c925))


### BREAKING CHANGES

* splitting address to server and client

## [2.0.3](https://github.com/James-Frowen/SimpleWebSocket/compare/v2.0.2...v2.0.3) (2022-04-27)


### Bug Fixes

* fixing ssl on client ([a6cad48](https://github.com/James-Frowen/SimpleWebSocket/commit/a6cad489871173679b2336f1caddf598f9ed4a0d))
* server should only use ssl for sslEnabled ([0a1a8e8](https://github.com/James-Frowen/SimpleWebSocket/commit/0a1a8e85221fd9f1a30c45931907f46f93d7e997))

## [2.0.2](https://github.com/James-Frowen/SimpleWebSocket/compare/v2.0.1...v2.0.2) (2022-04-27)


### Bug Fixes

* moving setup to constructor so that it throws if there is a problem instead of bind ([24cd0d6](https://github.com/James-Frowen/SimpleWebSocket/commit/24cd0d6c0e2256ec8a68ee3f44d86085686d1d42))

## [2.0.1](https://github.com/James-Frowen/SimpleWebSocket/compare/v2.0.0...v2.0.1) (2022-04-26)


### Bug Fixes

* fixing gui conflicts with multiplexsocket ([2a35b4c](https://github.com/James-Frowen/SimpleWebSocket/commit/2a35b4c29eeedaba24f109162f2faa799aff95c8))

# [2.0.0](https://github.com/James-Frowen/SimpleWebSocket/compare/v1.0.0...v2.0.0) (2022-03-20)


### Bug Fixes

* updating simplewebtransport to v1.3.0 ([f4237c6](https://github.com/James-Frowen/SimpleWebSocket/commit/f4237c6f06bef47826daafc31fb32c840dbb1d0e))


* feat!: adding MaxPacketSize for Mirage v120 ([254cf87](https://github.com/James-Frowen/SimpleWebSocket/commit/254cf875c1e10a6de4d513bb4a97c3aa5f1b68a8))


### BREAKING CHANGES

* requires upgrade to Mirage v120

# 1.0.0 (2021-12-16)


### Bug Fixes

* fixing release with empty commit ([223a8a4](https://github.com/James-Frowen/SimpleWebSocket/commit/223a8a421ac518022e43e785448ff3164f40fac0))
* fixing swt for mirage ([ae9b2a5](https://github.com/James-Frowen/SimpleWebSocket/commit/ae9b2a5234be628035d7a3dfffc8551eb1bae3dd))
* update to mirage v113.0.0 ([4b85240](https://github.com/James-Frowen/SimpleWebSocket/commit/4b85240538660cc23ab812ddd4e350362dab35f0))


### Features

* adding log level to inspector ([b5e8cbe](https://github.com/James-Frowen/SimpleWebSocket/commit/b5e8cbe3c0f15e37c13be09e18a0caa16f50442a))
