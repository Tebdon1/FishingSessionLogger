import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ArrayHelperService {

  constructor() { }

  // helper function to return first element of array matching object value or filter function
  firstOrDefault(collection, func) {
    return this.arrayWhere(collection, func)[0] || null;
  }

  arrayWhere(collection, filter) {
    switch (typeof filter) {
        case 'function':
          return collection.filter(filter);
        case 'object':
            for (const property of filter) {
                if (!filter.hasOwnProperty(property)) {
                  continue; // ignore inherited properties
                }

                collection = collection.filter(function (item) {
                    return item[property] === filter[property];
                });
            }
            return collection.slice(0); // copy the array
        default:
            throw new TypeError('func must be either a' +
                'function or an object of properties and values to filter by');
    }
  }
}
