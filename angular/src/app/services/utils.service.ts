import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})

export class UtilsService {
    constructor(

    ) {
    }

    public backgroundColours = [ 
    '#0aabf2',
    '#12b526',
    '#f2ce38',
    '#ed930f',
    '#9212d6',
    '#d31326',
    '#0fd6a2',
    '#96d320',
    '#a32473',
    '#558da0',
    '#51555c',
    '#989aa2',
    '#70757c',
    '#490cef',
    '#b63ecc',
    '#daa520',
    '#0a9944',
    '#88ba83',
    '#b27029',
    '#a3545b',
    '#545db2',
    '#0c69ef',
    '#8161b5',
    '#a1b239',
    '#e582e8',
    '#f46240',
    '#f45ba5',
    '#8686bf',
    '#6ccc64',
    '#d1663d'];

    dateTimeToIsoString(date) {
        var tzo = -date.getTimezoneOffset(),
            dif = tzo >= 0 ? '+' : '-',
            pad = function(num) {
                return (num < 10 ? '0' : '') + num;
            };
      
        return date.getFullYear() +
            '-' + pad(date.getMonth() + 1) +
            '-' + pad(date.getDate()) +
            'T' + pad(date.getHours()) +
            ':' + pad(date.getMinutes()) +
            ':' + pad(date.getSeconds()) +
            dif + pad(Math.floor(Math.abs(tzo) / 60)) +
            ':' + pad(Math.abs(tzo) % 60);
    }

    dateToIsoString(date) {
        const pad = function(num) {
            return (num < 10 ? '0' : '') + num;
        };

        return date.getFullYear() +
            '-' + pad(date.getMonth() + 1) +
            '-' + pad(date.getDate());
    }

    dateTimeToIsoStringAsUTC(date) {
        var pad = function(num) {
                return (num < 10 ? '0' : '') + num;
            };
      
        return date.getFullYear() +
            '-' + pad(date.getMonth() + 1) +
            '-' + pad(date.getDate()) +
            'T' + pad(date.getHours()) +
            ':' + pad(date.getMinutes()) +
            ':' + pad(date.getSeconds()) +
            '+00:00'
    }

    dateToShortFormat(date) {

        const monthNames = ["Jan", "Feb", "Mar", "Apr",
                            "May", "Jun", "Jul", "Aug",
                            "Sep", "Oct", "Nov", "Dec"];
        
        const day = date.getDate();
        
        const monthIndex = date.getMonth();
        const monthName = monthNames[monthIndex];
        
        const year = date.getFullYear();
        
        return `${day}-${monthName}-${year}`;  
    }

/* 

    dateToJsonString(dt: Date) {
        if (dt.getDate) {
            const dd = dt.getDate();
            const MM = dt.getMonth() + 1; // January is 0!

            const yyyy = dt.getFullYear();

            let ddS = dd.toString();
            let MMS = MM.toString();
            if (dd < 10) {
                ddS = '0' + ddS;
            }
            if (MM < 10) {
                MMS = '0' + MMS;
            }

            return yyyy.toString() + '-' + MMS + '-' + ddS;
        }
    }

    dateToJsonStringUTC(dt: Date) {
        if (dt.getDate) {
            return this.dateToJsonString(dt) + "Z";
        }
    }

    timeToJsonString(dt: Date) {
        if (dt.getDate) {
            const dd = dt.getDate();
            const MM = dt.getMonth() + 1; // January is 0!
            const HH = dt.getHours();
            const mm = dt.getMinutes();
            const ss = dt.getSeconds();

            const yyyy = dt.getFullYear();

            let ddS = dd.toString();
            let MMS = MM.toString();
            let HHS = HH.toString();
            let mmS = mm.toString();
            let ssS = ss.toString();
            if (dd < 10) {
                ddS = '0' + ddS;
            }
            if (MM < 10) {
                MMS = '0' + MMS;
            }
            if (HH < 10) {
                HHS = '0' + HHS;
            }
            if (mm < 10) {
                mmS = '0' + mmS;
            }
            if (ss < 10) {
                ssS = '0' + ssS;
            }

            return yyyy.toString() + '-' + MMS + '-' + ddS + ' ' + HHS + ':' + mmS + ':' + ssS;
        }
    } */

      convertBase64ToBlob(base64, type = "application/octet-stream" ) {
        const binStr = atob( base64 );
        const len = binStr.length;
        const arr = new Uint8Array(len);
        for (let i = 0; i < len; i++) {
            arr[ i ] = binStr.charCodeAt( i );
        }
        return new Blob( [ arr ], { type: type } );
    }

    convertBlobToBase64 = (blob: Blob) => new Promise<any>((resolve, reject) => {
        const reader = new FileReader;
        reader.onerror = reject;
        reader.onload = () => {
            resolve(reader.result);
        };
        reader.readAsDataURL(blob);
      });

    // RFC4122 version 4 compliant UUID generator.
    // Based on: http://stackoverflow.com/questions/105034/how-to-create-a-guid-uuid-in-javascript/2117523#2117523
    generateUUID() {
        let now = typeof Date.now === 'function' ? Date.now() : new Date().getTime();
        const uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
            const r = (now + Math.random()*16)%16 | 0;
            now = Math.floor(now/16);
            return (c=='x' ? r : (r&0x7|0x8)).toString(16);
        });
        return uuid;
    };

    formatDate(inputDate: Date) {
        var mMonth = inputDate.getMonth() + 1;
        var month = mMonth < 10 ? '0' + mMonth : mMonth;
        var date = inputDate.getDate() < 10 ? '0' + inputDate.getDate() : inputDate.getDate();
        var formatDate = date + '/' + month + '/' + inputDate.getFullYear();
        return formatDate;
    }      

    formatDateTime(inputDate: Date) {
        var sec = inputDate.getSeconds() < 10 ? '0' + inputDate.getSeconds() : inputDate.getSeconds();
        var minute = inputDate.getMinutes() < 10 ? '0' + inputDate.getMinutes() : inputDate.getMinutes();
        var hour = inputDate.getHours() < 10 ? '0' + inputDate.getHours() : inputDate.getHours();
        var mMonth = inputDate.getMonth() + 1;
        var month = mMonth < 10 ? '0' + mMonth : mMonth;
        var date = inputDate.getDate() < 10 ? '0' + inputDate.getDate() : inputDate.getDate();
        var formatDate = date + '/' + month + '/' + inputDate.getFullYear() + ' ' + hour + ':' + minute + ':' + sec;
        return formatDate;
    }      

    cleanBase64(contents: string): string {
        // strip off any image prefix from file contents
        if (contents.indexOf(';base64,') > -1) {
            return contents.substring(contents.lastIndexOf(';base64,') + ';base64,'.length);
        } 
        else {
            return contents;
        }
    }

    async waitForCondition(condition, timeout): Promise<any> {
        return new Promise((resolve, reject) => {
          if (timeout) {
            setTimeout(() => {
              reject("timeout");
            }, timeout);
          }
    
          const test = () => {
            setTimeout(() => {
              if (condition()) {
                resolve(true);
              } else {
                test();
              }
            }, 10);
          };
    
          test();
        });
    }

    currentDateWithNoTime() {
        var now = new Date();
        return new Date(now.getFullYear(),now.getMonth(),now.getDate());
    }
}
