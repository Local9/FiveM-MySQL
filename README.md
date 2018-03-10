# GHMattiMySQL
Another MySQL implementation for FiveM's FXServer with Multithreading.

## How to use in your resource?
* Add into the relevant `__resource.lua` the following line: `dependency 'GHMattiMySQL'`
* Configure the Resource by editing the `settings.xml` sensibly.
* If you want to use convars, the convars in the server.cfg you have to set are `mysql_connection_string` to a connection string like `"server=localhost;database=fivem;userid=ghmatti;password=password"` and `mysql_debug`
* Use `exports` to query your Database

## Exports
### Sync Exports
The sync exports all wait until the result is returned and then the code continues.
* `exports['GHMattiMySQL']:Query( querystring, [optional: parameters] )` Use this for INSERT, DELETE, and UPDATE. This currently awaits a result at the end, if you do not want that behaviour, use an Async export. If in doubt, use the Async export for this.
* `exports['GHMattiMySQL']:QueryResult( querystring, [optional: parameters] )` Use this for SELECT statements. It returns the rows, with the selected elements inside. You have no idea what it contains? Just `print(json.encode(result))` your results.
* `exports['GHMattiMySQL']:QueryScalar( querystring, [optional: parameters] )` returns a singular value only.
* `exports['GHMattiMySQL']:Transaction(list of query strings, [optional: parameters])` This function is used for transactions, which are essentially multiple linked INSERT / UPDATE / DELETE statements. If one fails the transaction will try and rollback all of them. If the rollback fails, the resource will crash intentionally. This export returns true or false depending on whether the transaction succeeded or failed.
### Async Exports
Async exports do not wait for the result to be returned, thus they do not return anything, but call the callback function when done.
* `exports['GHMattiMySQL']:QueryAsync( querystring, [optional: parameters, callback function] )` Use this for INSERT, DELETE, and UPDATE.
* `exports['GHMattiMySQL']:QueryResultAsync( querystring, [optional: parameters, callback function] )` Use this for SELECT statements. It returns the rows, with the selected elements inside as the parameter for your callback function. You have no idea what it contains? Just use `print(json.encode(result))` to see the results.
* `exports['GHMattiMySQL']:QueryScalarAsync( querystring, [optional: parameters, callback function] )` returns a singular value as parameter for your callback function only.
* `exports['GHMattiMySQL']:TransactionAsync(list of query strings, [optional: parameters, callback])` This function is used for transactions, which are essentially multiple linked INSERT / UPDATE / DELETE statements. If one fails the transaction will try and rollback all of them. If the rollback fails, the resource will crash intentionally. The callback gets return true or false depending on whether the transaction succeeded or failed.
* `exports['GHMattiMySQL']:Insert( tablename, rows-array, [optional: callback, bool return insert id] )` Use this function for multi-row inserts, it will write out a single command to do multiple inserts into tablename. The rows-array is constructed as illustrated by the following example: `{{["id"]=1,["name"]="John"},{["id"]=2,["name"]="Peter"},{["id"]=3,["name"]="Paul"}}`. The callback just returns the amount of rows inserted, if you put the optional boolean to true though, it will return the last insert id.

## Change Log
* **2018/02/17** *Version: 0.0.1:* Initial Release
* **2018/02/18** *Version: 0.0.2:* Added Parameter Handling, and Async exports for Lua.
* **2018/02/19** *Version: 0.0.3:* Better Debug Handling, Added Multi-Row Inserts, Made Callbacks optional for Async calls. Does not just throw/die anymore on faulty mysql Command syntax.
* **2018/02/20** *Version: 0.0.4:* Multi-Threading, Support for Convars, Fixed missing Parameter handling on the Scalar function.
* **2018/02/23** *Version: 0.5.0:* Added returning of Last Insert Id on Inserts via an optional parameter setting, limiting of thread usage, fixed a bug introduced by using dynamic instead of object, transactions, stringified debug querys with parameters, and jumped versions because I can.
* **2018/03/10** *Version: 0.5.1:* Updated MySqlConnector to 0.36.1, Changed the License to AGPL (if you change something and use it, share it), Added a Drag and Drop Replacer for mysql-async, which is not recommended to use.