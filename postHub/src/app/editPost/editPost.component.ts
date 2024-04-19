import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { faDownLong, faEllipsis, faMessage, faUpLong } from '@fortawesome/free-solid-svg-icons';
import { Hub } from '../models/hub';
import { HubService } from '../services/hub.service';
import { ActivatedRoute, Router } from '@angular/router';
import { PostService } from '../services/post.service';
import { Post } from '../models/post';

@Component({
  selector: 'app-editPost',
  templateUrl: './editPost.component.html',
  styleUrls: ['./editPost.component.css']
})
export class EditPostComponent implements OnInit {

  hub : Hub | null = null;
  postTitle : string = "";
  postText : string = "";

  @ViewChild("fileUploadViewChild", {static:false}) pictureInput ?: ElementRef;

  formData : FormData = new FormData();
  NbImage : number = -1;
  

  // Icônes Font Awesome
  faEllipsis = faEllipsis;
  faUpLong = faUpLong;
  faDownLong = faDownLong;
  faMessage = faMessage;

  constructor(public hubService : HubService, public route : ActivatedRoute, public postService : PostService, public router : Router) { }

  async ngOnInit() {
    let hubId : string | null = this.route.snapshot.paramMap.get("hubId");

    if(hubId != null){
      this.hub = await this.hubService.getHub(+hubId);
    }

  }

  // Créer un nouveau post (et son commentaire principal)
  async createPost(){
    if(this.postTitle == "" || this.postText == ""){
      alert("Remplis mieux le titre et le texte niochon");
      return;
    }
    if(this.hub == null) return;

    let index : number = 0;
    while(this.pictureInput?.nativeElement.files[index] != null)
    {
        let file = this.pictureInput?.nativeElement.files[index];
        this.formData.append(index.toString(), file, file.name);
        index = index + 1;
    }
    this.formData.append("title", this.postTitle);
    this.formData.append("text", this.postText);
    let newPost : Post = await this.postService.postPost(this.hub.id, this.formData);

    // On se déplace vers le nouveau post une fois qu'il est créé
    this.router.navigate(["/post", newPost.id]);
  }

  ajouterimage(){
    let file = this.pictureInput?.nativeElement.files[0];
    if(file == null)
    {
      console.log("Input HTML ne contient aucune image.");
      return;
    }

    this.NbImage = this.NbImage + 1;
    this.formData.append(this.NbImage.toString(),file,file.name);
  }

}
