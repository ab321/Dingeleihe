import { LibraryCustomer } from "./libraryCustomer";
import { Thing } from "./thing";

export class Lending{
    id: number = -1;
    libraryCustomerId: number = -1;
    libraryCustomer: LibraryCustomer = new LibraryCustomer(); 
    thingId: number = -1;
    thing: Thing = new Thing();
    lendBegin: Date = new Date();
    lendEnd: Date = new Date();
}