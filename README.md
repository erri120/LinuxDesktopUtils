# LinuxDesktopUtils

Utilities and wrappers for Linux desktop specific functionality.

## XDG Base Directories

The package `LinuxDesktopUtils.XDGBaseDirectories` implements version 0.8 of the [XDG Base Directory Specification](https://specifications.freedesktop.org/basedir-spec/basedir-spec-latest.html).

## XDG Desktop Portals

The package `LinuxDesktopUtils.XDGDesktopPortals` makes the following [XDG Desktop Portals](https://flatpak.github.io/xdg-desktop-portal/docs/api-reference.html) available in an easy-to-use API:

- [x] [Account](https://flatpak.github.io/xdg-desktop-portal/docs/doc-org.freedesktop.portal.Account.html) version 1
  - [x] `GetUserInformation`
- [ ] [File Chooser](https://flatpak.github.io/xdg-desktop-portal/docs/doc-org.freedesktop.portal.FileChooser.html) version 4
  - [x] `OpenFile`
  - [x] `SaveFile`
  - [ ] `SaveFiles`
- [ ] [Network Monitor](https://flatpak.github.io/xdg-desktop-portal/docs/doc-org.freedesktop.portal.NetworkMonitor.html) version 3
  - [x] `GetAvailable` added in version 2
  - [x] `GetMetered` added in version 2
  - [x] `GetConnectivity` added in version 2
  - [x] `GetStatus` added in version 3
  - [x] `CanReach` added in version 3
  - [ ] Signal: `changed`
- [x] [OpenURI](https://flatpak.github.io/xdg-desktop-portal/docs/doc-org.freedesktop.portal.OpenURI.html) version 5
  - [x] `OpenURI`
  - [x] `OpenFile`
  - [x] `OpenDirectory`
  - [x] `SchemeSupported` added in version 5
- [ ] [Screenshot](https://flatpak.github.io/xdg-desktop-portal/docs/doc-org.freedesktop.portal.Screenshot.html) version 2
  - [x] `Screenshot`
  - [ ] `PickColor`
- [x] [Secret](https://flatpak.github.io/xdg-desktop-portal/docs/doc-org.freedesktop.portal.Secret.html) version 1
  - [x] `RetrieveSecret`: Note that this method requires a static application ID, which can only be obtained correctly for sandboxed applications. As such, applications running directly on the host will likely get a new master secret with each restart. Applications running on the host should use `libsecret` directly.
- [x] [Trash](https://flatpak.github.io/xdg-desktop-portal/docs/doc-org.freedesktop.portal.Trash.html) version 1
  - [x] `TrashFile`

## License

See [LICENSE](./LICENSE).

