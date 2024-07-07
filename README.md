# LinuxDesktopUtils

Utilities and wrappers for Linux desktop specific functionality.

## XDG Base Directories

The package `LinuxDesktopUtils.XDGBaseDirectories` implements version 0.8 of the [XDG Base Directory Specification](https://specifications.freedesktop.org/basedir-spec/basedir-spec-latest.html).

## XDG Desktop Portals

The package `LinuxDesktopUtils.XDGDesktopPortals` makes the following [XDG Desktop Portals](https://flatpak.github.io/xdg-desktop-portal/docs/api-reference.html) available in an easy-to-use API:

- [ ] [File Chooser](https://flatpak.github.io/xdg-desktop-portal/docs/doc-org.freedesktop.portal.FileChooser.html) version 4
  - [x] `OpenFile`
  - [ ] `SaveFile`
  - [ ] `SaveFiles`
- [x] [OpenURI](https://flatpak.github.io/xdg-desktop-portal/docs/doc-org.freedesktop.portal.OpenURI.html) version 4
  - [x] `OpenURI`
  - [x] `OpenFile`
  - [x] `OpenDirectory`

## License

See [LICENSE](./LICENSE).

