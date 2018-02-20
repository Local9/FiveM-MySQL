## Version 0.0.4 [ 2018 / 02 / 20 ]
* Added Multithreading. This speeds up using many database querys quite a bit.
* Fixed the missing handling of Parameters by QueryScalar[Async].
* Added support for using Convars instead of definiting the server connection by the `settings.xml`
* Added an optional Callback to :Insert

## Version 0.0.3 [ 2018 / 02 / 19 ] 
* Better Debug Handling: MySQL errors do not lead to throwing and dying of the entire resource, it'll just display the MySQL error.
* Added Multi-Row Inserts: Basically a CommandText constructor for lazy people.
* Made Callbacks optional for Async calls: so if you are not using parameters, you can skip the warning if the Debug option is set to false

## Version 0.0.2 [ 2018 / 02 / 18 ]
### Changes:
* Added parameter support, which is backwards compatible, so you do not need to change your lua function calls.
* Parameters are tested if they are in the right shape. e.g. in Lua: `{["@id"] = 1}`, for the query: `SELECT username FROM users WHERE id = @id;`
* Added Async exports for Lua that do not wait for the C# response to finish, thus speeding up INSERT, DELETE and UPDATE calls considerably.

## Version 0.0.1 [ 2018 / 02 / 17 ]
### Initial Release
