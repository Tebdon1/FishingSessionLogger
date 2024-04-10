export enum ColumnDataType {
  String,
  Integer,
  Decimal,
  Date,
  Boolean,
  Enum,
  CollectionMultiple,
  // only real difference between the two collection types is the wording: multiple uses contains, single uses equals. 
  CollectionSingle
}
