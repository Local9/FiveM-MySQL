# Disclaimer
I do not support or condone the use of these files, it is just there because people have had repeatedly problems with mysql-async; and it is difficult for many of them to switch to a different mysql resource.

## Warning
I will neither help or support using this to overwrite the resource files. This is but a temporary solution, you **should** be using the exports given directly by the `GHMattiMySQL` resource.

# Instructions
* Move the `GHMattiMySQL` resource to your resources folder from the release section of this repository.
* Copy and Paste the content of this folder, except this file, to your servers resources folder; overwrite all files.
* Remove all `dependency 'mysql-async'` from your `__resource.lua` files, it might not be needed.
* Edit the `server.cfg`
* Enjoy my mysql implementation instead of `mysql-async`.

## Editing the `server.cfg`
* Remove `start mysql-async`
* Add `start GHMattiMySQL` at best at the very first position of all starts.

# Known Issues:
* Insert does not return the last inserted id, but affected rows. If you want that behaviour use the different Insert syntax from `GHMattiMySQL`
* The `mysql-async` implementation was far more forgiving on mysql queries (because of some not recommended settings). If you fuck up or use bad code, it is very likely that you will be confronted with errors and server crashes.
