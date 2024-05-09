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
  newAdmin : string | null = null;
  IsUserAdmin : boolean = false;
  IsUserModerator : boolean = false;

  constructor(public userService : UserService, public router : Router) { }

  async ngOnInit() {
    this.userIsConnected = localStorage.getItem("token") != null;
    this.username = localStorage.getItem("username");

     if(this.username != null)
    {
      this.IsUserAdmin = await this.userService.IsUserAdmin(this.username);

      if(!this.IsUserAdmin)
      {
        this.IsUserModerator = await this.userService.IsUserModerator(this.username);
      }
    }

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
          window.location.reload();
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
  window.location.reload();
 }

 async MakeModerator()
 {
  
  if(this.newAdmin != null)
    this.userService.MakeModerator(this.newAdmin);
  else
    alert("Le nom d'utilisateur est vide");  
 }
}