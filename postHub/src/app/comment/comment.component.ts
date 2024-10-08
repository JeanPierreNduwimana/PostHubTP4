import { Component, ElementRef, Input, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { faDownLong, faEllipsis, faImage, faL, faMessage, faUpLong, faXmark } from '@fortawesome/free-solid-svg-icons';
import { Comment } from '../models/comment';
import { PostService } from '../services/post.service';
import Glide from '@glidejs/glide';
import { Picture } from '../models/picture';

@Component({
  selector: 'app-comment',
  templateUrl: './comment.component.html',
  styleUrls: ['./comment.component.css']
})
export class CommentComponent implements OnInit {

  @Input() comment : Comment | null = null;

  // Icônes Font Awesome
  faEllipsis = faEllipsis;
  faUpLong = faUpLong;
  faDownLong = faDownLong;
  faMessage = faMessage;
  faImage = faImage;
  faXmark = faXmark;

  // Plein de variables sus pour afficher / cacher des éléments HTML
  replyToggle : boolean = false;
  editToggle : boolean = false;
  repliesToggle : boolean = false;
  isAuthor : boolean = false;
  editMenu : boolean = false;
  displayInputFile : boolean = false;

  // Variables associées à des inputs
  newComment : string = "";
  editedText ?: string;
  listImages : Picture[] = [];

  constructor(public postService : PostService) { }

  ngOnInit() {
    this.isAuthor = localStorage.getItem("username") == this.comment?.username;
    this.editedText = this.comment?.text;
  }

  @ViewChild("filesUploadByUser", {static:false}) pictureInput?: ElementRef;
  @ViewChildren('glideitems') glideitems : QueryList<any> = new QueryList();

  ngAfterViewInit() {
    this.glideitems.changes.subscribe(e => {
      this.initGlide();
    });
    if (this.glideitems.length > 0) {
      this.initGlide();
    }
  }
    

  initGlide() {
    var glide = new Glide('.glide', {
      type: 'carousel',
      focusAt: 'center',
      perView: Math.ceil(window.innerWidth / 400)
    });
    glide.mount();
  }
 

  // Créer un nouveau sous-commentaire au commentaire affiché dans ce composant
  // (Pouvoir les commentaires du post, donc ceux qui sont enfant du commentaire principal du post,
  // voyez le composant fullPost !)
  async createComment(){
    if(this.newComment == ""){
      alert("Écris un commentaire niochon !");
      return;
    }
    if(this.comment == null) return;
    if(this.comment.subComments == null) this.comment.subComments = [];

    //Pour envoyer les photos au serveurs.   
    let formdata = new FormData;
    let i : number = 0; 
    formdata.append("textComment", this.newComment)

    let file = this.pictureInput?.nativeElement.files[0];

    while(file != null && file != undefined){
      formdata.append(i.toString(), file, file.name);
      i++;
      file = this.pictureInput?.nativeElement.files[i];
    }
    
    this.comment.subComments.push(await this.postService.postComment(formdata, this.comment.id));

    this.replyToggle = false;
    this.repliesToggle = true;
    this.newComment = "";
  }

  // Modifier le texte (et éventuellement ajouter des images) d'un commentaire
  async editComment(){

    if(this.comment == null || this.editedText == undefined) return;

    let formdata = new FormData;
    let i : number = 0;
    formdata.append("text", this.editedText);
    let file = this.pictureInput?.nativeElement.files[0];
    while(file != null && file != undefined){
      formdata.append(i.toString(), file, file.name);
      i++;
      file = this.pictureInput?.nativeElement.files[i];
    }


    let newMainComment = await this.postService.editComment(formdata, this.comment.id);
    this.comment = newMainComment;
    this.editedText = this.comment.text;
    this.editMenu = false;
    this.editToggle = false;
  }

  // Supprimer un commentaire (le serveur va le soft ou le hard delete, selon la présence de sous-commentaires)
  async deleteComment(){
    if(this.comment == null || this.editedText == undefined) return;
    await this.postService.deleteComment(this.comment.id);

    // Changements visuels pour le soft-delete
    if(this.comment.subComments != null && this.comment.subComments.length > 0){
      this.comment.username = null;
      this.comment.upvoted = false;
      this.comment.downvoted = false;
      this.comment.upvotes = 0;
      this.comment.downvotes = 0;
      this.comment.text = "Commentaire supprimé.";
      this.isAuthor = false;
    }
    // Changements ... visuels ... pour le hard-delete
    else{
      this.comment = null;
    }
  }

  // Upvoter (notez que ça annule aussi tout downvote fait pas soi-même)
  async upvote(){
    if(this.comment == null) return;
    await this.postService.upvote(this.comment.id);
    
    // Changements visuels immédiats
    if(this.comment.upvoted){
      this.comment.upvotes -= 1;
    }
    else{
      this.comment.upvotes += 1;
    }
    this.comment.upvoted = !this.comment.upvoted;
    if(this.comment.downvoted){
      this.comment.downvoted = false;
      this.comment.downvotes -= 1;
    }
  }

  // Upvoter (notez que ça annule aussi tout upvote fait pas soi-même)
  async downvote(){
    if(this.comment == null) return;
    await this.postService.downvote(this.comment.id);

    // Changements visuels immédiats
    if(this.comment.downvoted){
      this.comment.downvotes -= 1;
    }
    else{
      this.comment.downvotes += 1;
    }
    this.comment.downvoted = !this.comment.downvoted;
    if(this.comment.upvoted){
      this.comment.upvoted = false;
      this.comment.upvotes -= 1;
    }
  }

  //Delete a picture
  async DeletePicutre(id : number){
    if(this.isAuthor != false){
      console.log("Est auteur")
      if(this.comment != undefined){
        if(id != undefined){
          this.listImages = this.comment.pictures;
          for(let i = 0; i <= this.listImages.length; i++){
            let image : Picture = this.listImages[i]
            if(image.id == id){
              this.listImages.splice(i, 1);
              break;
            }
          }
          this.postService.supprimerPhoto(this.comment?.id, id);
          console.log("Réussie")
        }
      }
    }
  }

  async SignalementPost(id : number){
    if(id != undefined){
      this.comment?.isReported == true;
      this.postService.report(id);
    }
  }
}
