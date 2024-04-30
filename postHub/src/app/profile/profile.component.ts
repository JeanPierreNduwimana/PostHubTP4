import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { UserService } from '../services/user.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {

  userIsConnected : boolean = false;

  // Vous êtes obligés d'utiliser ces trois propriétés
  oldPassword : string = "";
  newPassword : string = "";
  newPasswordConfirm : string = "";

  username : string | null = null;

  constructor(public userService : UserService, public router : Router) { }

  ngOnInit() {
    this.userIsConnected = localStorage.getItem("token") != null;
    this.username = localStorage.getItem("username");
  }

  @ViewChild("fileUploadViewChild", {static:false}) pictureInput?: ElementRef;

  async ChangeAvatar(){
    let fileInput = this.pictureInput?.nativeElement;
    if (fileInput && fileInput.files && fileInput.files.length > 0) {
      let file = fileInput.files[0];
      let formData = new FormData();
      formData.append("UserNewAvatar", file, file.name);
      
      if (this.username) {
        try {
          await this.userService.changeAvatar(this.username, formData);
          console.log("Avatar changed successfully");
        } catch (error) {
          console.error("Error while changing avatar:", error);
        }
      }
    } else {
      console.error("No file selected");
    }
 }

 async ChangerMotDePasse(){

  if(this.newPassword != this.newPasswordConfirm)
  {
    alert("Mot de passe non-identiques");
    return;
  }

  let formData = new FormData();
  formData.append("oldPassword", this.oldPassword);
  formData.append("newPassword",this.newPassword)
  await this.userService.ChangerMotDePasse(formData);
  this.router.navigate(["/postList", "index"]);
 }
}