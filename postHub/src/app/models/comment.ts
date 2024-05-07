import { Picture } from "./picture";
import { publishFacade } from "@angular/compiler";
export class Comment{
    constructor(
        public id : number,
        public text : string,
        public date : Date,
        public username : string | null,
        public upvotes : number,
        public downvotes : number,
        public upvoted : boolean,
        public downvoted : boolean,
        public subCommentTotal : number,
        public subComments : Comment[] | null,
        public pictures : Picture[],
        public isReported : boolean
    ){}
}