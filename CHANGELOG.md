## Version 0.5.3 [ 2018 / 03 / 13 ]
* Fixed an even more urgent bug introduced by the bugfix yesterday.

## Version 0.5.2 [ 2018 / 03 / 12 ]
* Fixed an urgent bug where QueryScalar would not return null on System.DBNull

## Version 0.5.1 [ 2018 / 03 / 10 ]
* Updated to MySqlConnector 0.36.1
* Added a Drag and Drop Replacer for `mysql-async`

## Version 0.5.0 [ 2018 / 02 / 23 ]
* Insert now has another optional parameter, to indicate if it should return the last inserted id by the statement. The id needs to be an auto incrementing value.
* Added an optional console variable / convar `mysql_thread_limit`, which can be set in the server.cfg if you want to limit or control multithreading better.
* Fixed an bug where QueryScalar[Async] would crash the resource when it returned null, as the server tries to unwrap nothing. Do not use dynamic and send null with it folks.
* Added Transactions.
* Changed so Debug mode would show querys stringified. Not correctly, but probably enough to debug your queries.
* Jumped the Version, because I am almost out of ideas to add.

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
