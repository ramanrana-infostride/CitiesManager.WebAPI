import { Component } from '@angular/core';
import { City } from '../models/city';
import { CitiesService } from '../services/cities.service';
import { FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-cities',
  templateUrl: './cities.component.html',
  styleUrls: ['./cities.component.css']
})
export class CitiesComponent {
    cities : City[] = [];
    postCityForm : FormGroup;

    constructor(private citiesService : CitiesService)
    {
      this.postCityForm = new FormGroup(
        {
          cityName : new FormControl(null ,[Validators.required])
        }
      );
    }

    loadCities()
    {
      //Get Cities
       this.citiesService.getCities()
       .subscribe({
      next : (response : City[]) =>  {
          this.cities = response;
        },
       error : (error : any) =>  {
          console.log(error);
        },
        complete : () => {}
      })

    }

    ngOnInit()
    {
      this.loadCities();
    }


    get postCity_CityNameControl () : any 
    {
      return this.postCityForm.controls["cityName"];
    }

    postCitySubmitted()
    {
      //ToDo : Add Logic here 

    }


}
