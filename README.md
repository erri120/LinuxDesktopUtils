# LinuxDesktopUtils

Utilities and wrappers for Linux desktop specific functionality.

## XDG Base Directories

The package `LinuxDesktopUtils.XDGBaseDirectories` implements version 0.8 of the [XDG Base Directory Specification](https://specifications.freedesktop.org/basedir-spec/basedir-spec-latest.html).

## XDG Desktop Portals

The package `LinuxDesktopUtils.XDGDesktopPortals` makes the following [XDG Desktop Portals](https://flatpak.github.io/xdg-desktop-portal/docs/api-reference.html) available in an easy-to-use API:

- [ ] [File Chooser](https://flatpak.github.io/xdg-desktop-portal/docs/doc-org.freedesktop.portal.FileChooser.html) version 4
    - [x] `OpenFile`
    - [x] `SaveFile`
    - [ ] `SaveFiles`
- [x] [OpenURI](https://flatpak.github.io/xdg-desktop-portal/docs/doc-org.freedesktop.portal.OpenURI.html) version 4
    - [x] `OpenURI`
    - [x] `OpenFile`
    - [x] `OpenDirectory`
- [x] [Secret](https://flatpak.github.io/xdg-desktop-portal/docs/doc-org.freedesktop.portal.Secret.html) version 1
    - [x] `RetrieveSecret`: Note that this method requires a static application ID, which can only be obtained correctly for sandboxed applications. As such, applications running directly on the host will likely get a new master secret with each restart. Applications running on the host should use `libsecret` directly.

## License

See [LICENSE](./LICENSE).

