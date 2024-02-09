# YC.DynamicsMapper Source Generator Release Notes

## v1.0.9
* Fixed CS8627 error on `IEntityMapper<T>`. (nullable type constraint)


## v1.1.0 
This version has some major changes.

* Added  new Class `DynamicsMapperSettings`
* Added new inetface `IPropertyMappers` 
* Added `DynamicsMapperSettings`. with the posibility to ignore default values when mapping a model to `Entity`
* Added to `DynamicsMapperSettings` the propperty `PropertyMappers` wich implements the `IPropertyMappers` inteface.
* Using the mappers interfaces to map the different mapping types to support the new settings and mappers