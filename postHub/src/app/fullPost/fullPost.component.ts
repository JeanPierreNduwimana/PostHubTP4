import { Component, ElementRef, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { faDownLong, faEllipsis, faImage, faMessage, faUpLong, faXmark } from '@fortawesome/free-solid-svg-icons';
import { Post } from '../models/post';
import { ActivatedRoute, Router } from '@angular/router';
import { PostService } from '../services/post.service';
import { Picture } from '../models/picture';
import Glide from '@glidejs/glide';

@Component({
  selector: 'app-fullPost',
  templateUrl: './fullPost.component.html',
  styleUrls: ['./fullPost.component.css']
})
export class FullPostComponent implements OnInit {

  // Variables pour l'affichage ou associées à des inputs
  post : Post | null = null;
  sorting : string = "popular";
  newComment : string = "";
  newMainCommentText : string = "";
  listImages : Picture[] = [];
  listBinImages : Picture[] = [];

  // Booléens sus pour cacher / afficher des boutons
  isAuthor : boolean = false;
  editMenu : boolean = false;
  displayInputFile : boolean = false;
  toggleMainCommentEdit : boolean = false;

  // Icônes Font Awesome
  faEllipsis = faEllipsis;
  faUpLong = faUpLong;
  faDownLong = faDownLong;
  faMessage = faMessage;
  faImage = faImage;
  faXmark = faXmark;

  constructor(public postService : PostService, public route : ActivatedRoute, public router : Router) { }

  async ngOnInit() {
    let postId : string | null = this.route.snapshot.paramMap.get("postId");

    if(postId != null){
      this.post = await this.postService.getPost(+postId, this.sorting);
      console.log(this.post);
      this.newMainCommentText = this.post.mainComment == null ? "" : this.post.mainComment.text;

      if(this.post.mainComment != null)
      {
        this.listImages = this.post.mainComment.pictures;
      }
      
    }
    this.isAuthor = localStorage.getItem("username") == this.post?.mainComment?.username;
  }

  @ViewChild("commentWithPicture", {static:false}) pictureInput?: ElementRef;
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
      perView: Math.ceil(window.innerWidth / 200)
    });
    glide.mount();
  }

  public annulerModification()
  {
    this.toggleMainCommentEdit = false;
    if(this.listBinImages.length > 0)
    {
      this.listBinImages.forEach( p =>
        {
          this.listImages.push(p);
        })
    }

    this.listBinImages = [];
  }
  
  public async removepicture(id : number) {

    for( let i : number = 0; i < this.listImages.length; i++)
    {
      if(this.listImages[i].id == id)
      {
        this.listBinImages.push(this.listImages[i]);
        this.listImages.splice(i,1);
        console.log(this.listBinImages);
        return;
      }
    }

  }

  async toggleSorting(){
    if(this.post == null) return;
    this.post = await this.postService.getPost(this.post.id, this.sorting);
  }

  // Créer un commentaire directement associé au commentaire principal du post
  async createComment(){
    if(this.newComment == ""){
      alert("Écris un commentaire niochon");
      return;
    }

    //Pour envoyer les photos au serveurs.
    let formdata = new FormData;
    let i : number = 0; 
    formdata.append("textComment", this.newComment)
    console.log(this.pictureInput?.nativeElement.filesUploadByUser)
    let file = this.pictureInput?.nativeElement.files[0];
    while(file != null && file != undefined){
      formdata.append(i.toString(), file, file.name);
      i++;
      file = this.pictureInput?.nativeElement.files[i];
      
    }
    this.post?.mainComment?.subComments?.push(await this.postService.postComment(formdata, this.post.mainComment.id));
    this.newComment = "";
  }

  // Upvote le commentaire principal du post
  async upvote(){
    if(this.post == null || this.post.mainComment == null) return;
    await this.postService.upvote(this.post.mainComment.id);
    if(this.post.mainComment.upvoted){
      this.post.mainComment.upvotes -= 1;
    }
    else{
      this.post.mainComment.upvotes += 1;
    }
    this.post.mainComment.upvoted = !this.post.mainComment.upvoted;
    if(this.post.mainComment.downvoted){
      this.post.mainComment.downvoted = false;
      this.post.mainComment.downvotes -= 1;
    }
  }

  // Downvote le commentaire principal du post
  async downvote(){
    if(this.post == null || this.post.mainComment == null) return;
    await this.postService.downvote(this.post.mainComment.id);
    if(this.post.mainComment.downvoted){
      this.post.mainComment.downvotes -= 1;
    }
    else{
      this.post.mainComment.downvotes += 1;
    }
    this.post.mainComment.downvoted = !this.post.mainComment.downvoted;
    if(this.post.mainComment.upvoted){
      this.post.mainComment.upvoted = false;
      this.post.mainComment.upvotes -= 1;
    }
  }

  // Modifier le commentaire principal du post
  async editMainComment(){

    let formdata = new FormData;
    let i : number = 0; 
    let file = this.pictureInput?.nativeElement.files[0];
    let listpictures : Picture[] = [];
     if(file == null)
    {
      console.log("aille");
      //return;
    }
    else{
      
       while(file != null && file != undefined){
      formdata.append(i.toString(), file, file.name);
      i++;
      file = this.pictureInput?.nativeElement.files[i];
    }
      //listpictures = await this.postService.AddPictures(formdata);
     // console.log(listpictures);
    }
 

    formdata.append("text",this.newMainCommentText);

    if(this.post == null || this.post.mainComment == null) return;

   /*  let commentDTO = {
      text : this.newMainCommentText,
      pictures : listpictures
    } */

    

    if(this.listBinImages.length > 0)
    {
      await this.postService.deletePictures(this.listBinImages);
    }
    
    let newMainComment = await this.postService.editComment(formdata, this.post?.mainComment.id);
    this.post.mainComment = newMainComment;
    this.toggleMainCommentEdit = false;

    

    this.listBinImages = [];
    this.listImages = [];
    this.listImages = newMainComment.pictures;
    console.log(this.listImages);
  }

  // Supprimer le commentaire principal du post. Notez que ça ne va pas supprimer le post en entier s'il y a le moindre autre commentaire.
  async deleteComment(){
    if(this.post == null || this.post.mainComment == null) return;
    await this.postService.deleteComment(this.post.mainComment.id);
    this.router.navigate(["/"]);
  }

  //Delete a picture
  async DeletePicutre(id : number){
      if(this.isAuthor != false){
        console.log("Est auteur")
        if(this.post != undefined){
          if(id != undefined){
            for(let i = 0; i <= this.listImages.length; i++){
              let image : Picture = this.listImages[i]
              if(image.id == id){
                this.listImages.splice(i, 1);
                break;
              }
            }
            this.postService.supprimerPhotoPost(this.post?.id, id);
            console.log("Réussie")
          }
        }
      }
  }

  async SignalementPost(id : number){
    if(id != undefined){
      this.postService.report(id);
      console.log(this.post?.mainComment?.id)
    }
  }
}